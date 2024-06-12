using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Microsoft.Extensions.Options;
using XeroApp.Services;
using XeroApp.Utilities;

namespace XeroApp.Filters
{
    public class TenantTimeoutAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var svc = context.HttpContext.RequestServices;
            var _xeroSvc = svc.GetService<IXeroService>();
            var _httpContextAccessor = svc.GetService<IHttpContextAccessor>();

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var accessToken = _xeroSvc.GetTokenAsync(emailAddress);
            var tenID =  _xeroSvc.GetTenantIdAsync(accessToken, emailAddress);

            var xeroConfig = svc.GetService<IOptions<XeroConfiguration>>();

            var client = new XeroClient(xeroConfig.Value);
            var ListOfSelectedTenants = client.GetConnectionsAsync(TokenUtilities.GetStoredToken(emailAddress)).Result;
            var isTenantActive = ListOfSelectedTenants.Any(t => t.TenantId.ToString().Equals(tenID));

            if (isTenantActive == false)
            {
                var clientState = Guid.NewGuid().ToString();
                TokenUtilities.StoreState(clientState);
                context.Result = new RedirectResult((client.BuildLoginUri(clientState)));
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
