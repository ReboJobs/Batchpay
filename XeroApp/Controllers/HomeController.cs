using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using System.Diagnostics;
using System.Net;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Models;
using Xero.NetStandard.OAuth2.Token;
using XeroApp.Extensions;
using XeroApp.Models;
using XeroApp.Services.ApiClientService;
using XeroApp.Models.BusinessModels;
using XeroApp.Services.UserTrackService;
using XeroApp.Services;
using XeroApp.Utilities;

namespace XeroApp.Controllers
{

    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IAccountingApi _accountingApi;
        private readonly IOptions<XeroConfiguration> XeroConfig;
        private readonly IXeroService _xeroSvc;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISession _session;

        public HomeController(ILogger<HomeController> logger, IAccountingApi accountingApi, IHttpContextAccessor httpContextAccessor, IOptions<XeroConfiguration> XeroConfig, IXeroService xeroSvc)
        {
            _logger = logger;
            _accountingApi = accountingApi;
            this.XeroConfig = XeroConfig;
            _xeroSvc = xeroSvc;
            _httpContextAccessor = httpContextAccessor;
            _session = _httpContextAccessor.HttpContext.Session;
            TokenUtilities.InitializedXeroService(_xeroSvc);

        }


        [AllowAnonymous]
        public async Task<IActionResult> Index([FromQuery] Guid? tenantId)
        {
            bool firstTimeConnection = false;


            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;
            var isTokenExist = TokenUtilities.TokenExists(emailAddress);
            if (isTokenExist)
            {
                firstTimeConnection = true;
            }

            if (tenantId is Guid tenantIdValue)
            {
                TokenUtilities.StoreTenantId(tenantIdValue, emailAddress);
            }

            if (firstTimeConnection)
            {
                return RedirectToAction("BatchPayments", "xero");
            }
            return View();
        }


        [Route("Home/Index/{tenantId}/{actionToRedirect}/{controllerToRedirect}")]
        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Index(Guid? tenantId, string actionToRedirect, string controllerToRedirect)
        {
            bool firstTimeConnection = false;

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;

            if (TokenUtilities.TokenExists(emailAddress))
            {
                firstTimeConnection = true;
            }

            if (tenantId is Guid tenantIdValue)
            {
                TokenUtilities.StoreTenantId(tenantIdValue, emailAddress);

            }

            if (firstTimeConnection)
            {
                return RedirectToAction(actionToRedirect, controllerToRedirect);
            }
            return View();
        }


        [HttpGet]

        public IActionResult getUserNameAbbv()
        {
            bool firstTimeConnection = false;

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;

            var AbbvName = string.Concat(session.getSessionGivenName().Substring(0, 1).ToUpper(), session.getSessionFamilyName().Substring(0, 1).ToUpper());

            return Json(AbbvName);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}