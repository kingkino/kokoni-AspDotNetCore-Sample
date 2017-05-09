using kokoni_transfer.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

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
                filterContext.Result = new RedirectResult("~/Login/Index/1001");
                return;
            }
            base.OnActionExecuting(filterContext);
        }
    }
}
