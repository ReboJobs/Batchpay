using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Client;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net.Http;
using XeroApp.Utilities;
using XeroApp.Services.OrganisationService;
using XeroApp.Models.BusinessModels;

namespace XeroApp.Controllers
{
    public class OrganisationInfo : Controller
    {

        private readonly ILogger<AuthorizationController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;
        private readonly IHttpClientFactory httpClientFactory;
        private readonly IOrganisationService _organisationService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISession _session;

        public OrganisationInfo(IOptions<XeroConfiguration> XeroConfig, IHttpContextAccessor httpContextAccessor, IHttpClientFactory httpClientFactory, ILogger<AuthorizationController> logger, IOrganisationService organisationService)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
            this.httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            _session = _httpContextAccessor.HttpContext.Session;
            _organisationService = organisationService;
        }

        // GET: /Organisation/
        public async Task<ActionResult> Index()
        {

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var xeroToken = TokenUtilities.GetStoredToken(emailAddress);
            var utcTimeNow = DateTime.UtcNow;
          
            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                var client = new XeroClient(XeroConfig.Value, (HttpClient)httpClientFactory);
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                emailAddress = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "email").Value;
                TokenUtilities.StoreToken(xeroToken, emailAddress);
            }

            string accessToken = xeroToken.AccessToken;
            string xeroTenantId = xeroToken.Tenants[0].TenantId.ToString();

            var AccountingApi = new AccountingApi();
            var response = await AccountingApi.GetOrganisationsAsync(accessToken, xeroTenantId);

            var organisation_info = new Organisation();
            organisation_info = response._Organisations[0];

            return View(organisation_info);
        }

        [Route("OrganisationInfo/GetOrganisationDetails/{tenantId}")]
        [HttpGet]
        public async Task<JsonResult> GetOrganisationDetails(string tenantId)
        {


            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var xeroToken = TokenUtilities.GetStoredToken(emailAddress);
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                var client = new XeroClient(XeroConfig.Value, (HttpClient)httpClientFactory);
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                emailAddress = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "email").Value;
                TokenUtilities.StoreToken(xeroToken, emailAddress);
            }

            var result = await _organisationService.GetOrganisationByTenantId(tenantId , xeroToken.AccessToken);
            return Json(result);
        }

        public async Task<ActionResult> OrganisationSettings()
        {

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;

            if (!TokenUtilities.TokenExists(emailAddress) || emailAddress == null)
            {
                return RedirectToAction("Index", "Home");
            }

            var xeroToken = TokenUtilities.GetStoredToken(emailAddress);
            var utcTimeNow = DateTime.UtcNow;
            var clientExpiry = new XeroClient(XeroConfig.Value);

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                xeroToken = (XeroOAuth2Token)await clientExpiry.RefreshAccessTokenAsync(xeroToken);
                emailAddress = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "email").Value;
                TokenUtilities.StoreToken(xeroToken, emailAddress);
            }

            var tenantId = TokenUtilities.GetCurrentTenantId(emailAddress);
            var client = new XeroClient(XeroConfig.Value);
            var ListOfSelectedTenants = await client.GetConnectionsAsync(TokenUtilities.GetStoredToken(emailAddress));



            ViewBag.OrgPickerTenantList = ListOfSelectedTenants.Join(xeroToken.Tenants, t => t.TenantId, st => st.TenantId, (t, st) => new TenantDetails
            {
                TenantName = t.TenantName,
                TenantId = t.TenantId
            }).ToList();

            ViewBag.OrgPickerCurrentTenantId = tenantId;
 

            ViewBag.actionToRedirect = "OrganisationSettings";
            ViewBag.controllerToRedirect = "OrganisationInfo";
            return View();
        }



        [HttpPost]
        public async Task<IActionResult> UpdateOrganisationSettings([FromBody] XeroTenantModel obj)
        {
            try
            {
                SessionUtility session = new SessionUtility(_httpContextAccessor);
                var emailAddress = session.getSession();
                ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;

                if (!TokenUtilities.TokenExists(emailAddress) || emailAddress == null)
                {
                    return RedirectToAction("Index", "Home");
                }

                var xeroToken = TokenUtilities.GetStoredToken(emailAddress);
                var utcTimeNow = DateTime.UtcNow;
                var clientExpiry = new XeroClient(XeroConfig.Value);

                if (utcTimeNow > xeroToken.ExpiresAtUtc)
                {
                    xeroToken = (XeroOAuth2Token)await clientExpiry.RefreshAccessTokenAsync(xeroToken);
                    emailAddress = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "email").Value;
                    TokenUtilities.StoreToken(xeroToken, emailAddress);
                }

                await _organisationService.UpdateOrganisationSettingsAsync(obj);
                return Json(new { Message = "Success", IsError = false });

            }
            catch (Exception ex)
            {

                return Json(new { Message = ex.Message, IsError = true });
            }
        }
    }
}