using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.SqlServer;
using kokoni_transfer.Data;
using kokoni_transfer.Models;
using kokoni_transfer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection.Extensions;
using kokoni_transfer.Helpers;
using Microsoft.Extensions.Options;

namespace kokoni_transfer
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);

            if (env.IsDevelopment())
            {
                // For more details on using the user secret store see https://go.microsoft.com/fwlink/?LinkID=532709
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            //services.AddDbContext<ApplicationDbContext>(options =>
            //    options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));


            // string constring = Configuration.GetConnectionString("DbConnection");

            string constring = Configuration["ConnectionsString:DbConnection"];

            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(constring));

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            //services.AddMvc();

            services.AddMvc(options =>
            {
                options.SslPort = 44356;
                options.Filters.Add(new RequireHttpsAttribute());
            });

            // Adds a default in-memory implementation of IDistributedCache.
            // services.AddDistributedMemoryCache();

            // SQLServerでsessionState
            services.AddDistributedSqlServerCache(options =>
            {
                //options.ConnectionString = Configuration["ConnectionsString:DbConnection"];
                options.ConnectionString = constring;
                options.SchemaName = "dbo";
                options.TableName = "SQLSessions";
            });

            services.AddSession(options =>
            {
                options.CookieName = ".Session";
                options.IdleTimeout = TimeSpan.FromMinutes(30);
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            var appSettings = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            string Facebook_AppId = Configuration.GetSection("AppSettings").GetValue<string>("Facebook_AppId");
            string Facebook_AppSecret = Configuration.GetSection("AppSettings").GetValue<string>("Facebook_AppId");
            string Twitter_ConsumerKey = Configuration.GetSection("AppSettings").GetValue<string>("Twitter_ConsumerKey");
            string Twitter_ConsumerSecret = Configuration.GetSection("AppSettings").GetValue<string>("Twitter_ConsumerSecret");
            
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();

                Facebook_AppId = Configuration["Authentication:Facebook_AppId"];
                Facebook_AppSecret = Configuration["Authentication:Facebook_AppSecret"];
                Twitter_ConsumerKey = Configuration["Authentication:Twitter_ConsumerKey"];
                Twitter_ConsumerSecret = Configuration["Authentication:Twitter_ConsumerSecret"];
            }
            else
            {
                app.UseExceptionHandler("/Dashboard/Error");                
            }

            app.UseStaticFiles();

            app.UseIdentity();

            app.UseSession();

            HttpHelper.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());

            // Microsoft
            //app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions()
            //{
            //    ClientId = Configuration["Authentication:Microsoft:ClientId"],
            //    ClientSecret = Configuration["Authentication:Microsoft:ClientSecret"]
            //});

            // Google 
            //app.UseGoogleAuthentication(new GoogleOptions()
            //{
            //    ClientId = Configuration["Authentication:Google:ClientId"],
            //    ClientSecret = Configuration["Authentication:Google:ClientSecret"]
            //});


            // Facebook
            app.UseFacebookAuthentication(new FacebookOptions()
            {
                AppId = Facebook_AppId,
                AppSecret = Facebook_AppSecret
            });

            // Twitter
            app.UseTwitterAuthentication(new TwitterOptions()
            {
                ConsumerKey = Twitter_ConsumerKey,
                ConsumerSecret = Twitter_ConsumerSecret
            }); 

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Login}/{action=Index}/{id?}");
            });
        }
    }
}
