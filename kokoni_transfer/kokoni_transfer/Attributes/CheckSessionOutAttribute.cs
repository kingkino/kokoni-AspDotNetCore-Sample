using kokoni_transfer.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;

namespace kokoni_transfer.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class CheckSessionOutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            byte[] value;
            if (!HttpHelper.HttpContext.Session.TryGetValue("OperationTime",out value))
            {
                var query = Base64UrlTextEncoder.Encode(Encoding.ASCII.GetBytes("1001"));
                filterContext.Result = new RedirectResult($"~/Login/Index?ec={query}");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
