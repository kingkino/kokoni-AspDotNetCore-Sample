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
                builder.AddUserSecrets<Startup>();
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
            CurrentEnvironment = env;
        }

        // 設定情報の取得
        private IConfigurationRoot Configuration { get; }

        // ホスト環境の設定情報の取得
        private IHostingEnvironment CurrentEnvironment { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // DBの接続文字列を追加
            string ConnectionString;

            // 開発環境用設定
            if (CurrentEnvironment.IsDevelopment())
            {
                ConnectionString = Configuration["ConnectionsStrings:DbConnection"];
                services.AddMvc(options =>
                {
                    options.SslPort = 44394;
                    options.Filters.Add(new RequireHttpsAttribute());
                });
            }
            else
            {
                ConnectionString = Configuration.GetConnectionString("DbConnection");
                services.AddMvc();
            }
            
            // DBコンテキストの設定
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(ConnectionString));

            // Identityの設定
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
                    {
                        options.Password.RequireDigit = true;
                        options.Password.RequireLowercase = true;
                        options.Password.RequireNonAlphanumeric = false;
                        options.Password.RequireUppercase = true;
                        options.User.AllowedUserNameCharacters = null;
                        options.User.RequireUniqueEmail = false;
                    }
                )
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // SQLServerでsessionState
            services.AddDistributedSqlServerCache(options =>
            {
                options.ConnectionString = ConnectionString;
                options.SchemaName = "dbo";
                options.TableName = "SQLSessions";
            });

            // Session情報の設定
            services.AddSession(options =>
            {
                options.CookieName = ".Session";
                options.IdleTimeout = TimeSpan.FromMinutes(1);
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();

            // 型を指定して設定情報を取得する場合はこの方法
            // IOptionsで設定情報を取得できる
            var appSettings = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettings);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/Dashboard/Error");                
            }

            // 静的ファイルの使用
            app.UseStaticFiles();

            app.UseIdentity();

            app.UseSession();

            HttpHelper.Configure(app.ApplicationServices.GetRequiredService<IHttpContextAccessor>());


            // Microsoft
            //app.UseMicrosoftAccountAuthentication(new MicrosoftAccountOptions()
            //{
            //    ClientId = Configuration["Authentication:Microsoft_ClientId"],
            //    ClientSecret = Configuration["Authentication:Microsoft_ClientSecret"]
            //});

            // Google 
            app.UseGoogleAuthentication(new GoogleOptions()
            {
                ClientId = Configuration["Authentication:Google_ClientId"],
                ClientSecret = Configuration["Authentication:Google_ClientSecret"]
            });

            // Facebook
            app.UseFacebookAuthentication(new FacebookOptions()
            {
                AppId = Configuration["Authentication:Facebook_AppId"],
                AppSecret = Configuration["Authentication:Facebook_AppSecret"]
            });

            // Twitter
            app.UseTwitterAuthentication(new TwitterOptions()
            {
                ConsumerKey = Configuration["Authentication:Twitter_ConsumerKey"],
                ConsumerSecret = Configuration["Authentication:Twitter_ConsumerSecret"]
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
