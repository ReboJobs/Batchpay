using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XeroApp.Utilities;

namespace XeroApp.Filters
{
    public class SessionTimeoutAttribute : ActionFilterAttribute
    {
   
        public SessionTimeoutAttribute()
        {
         
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            if (context.HttpContext.Session.GetString("xero_userid") == null ||
                context.HttpContext.Session.GetString("EmailAddress") == null ||
                context.HttpContext.Session.GetString("given_name") == null ||
                context.HttpContext.Session.GetString("family_name") == null)
            {
                
                context.Result = new RedirectResult("~/Authorization/Disconnect");
                return;
            }
            base.OnActionExecuting(context);
        }

    }
}
