using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Token;
using Xero.NetStandard.OAuth2.Models;
using Core;
using XeroApp.Services;
using Xero;
using AutoMapper;
using XeroApp.Models.BusinessModels;
using XeroApp.Services.ApiClientService;
using XeroApp.Services.UserTrackService;
using XeroApp.Services.OrganisationService;
using XeroApp.Utilities;



namespace XeroApp.Controllers
{
    public class AuthorizationController : Controller
    {
        private readonly ILogger<AuthorizationController> _logger;
        private readonly IOptions<XeroConfiguration> XeroConfig;
        //private readonly IHttpClientFactory _httpClientFactory;
        private readonly IXeroService _xeroSvc;
        private readonly IMapper _mapper;
        private readonly IApiClientService _apiClientSvc;
        private readonly IUserTrackService _userSvcTrack;
        private UserTrackModel userTrack = new UserTrackModel();
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISession _session;
        private readonly IOrganisationService _organisationService;
        private IConfiguration _configuration { get; set; }

        public AuthorizationController(
            IOptions<XeroConfiguration> XeroConfig,
            //IHttpClientFactory httpClientFactory,
            ILogger<AuthorizationController> logger,
            IXeroService xeroSvc,
            IApiClientService apiClientSvc,
            IUserTrackService userSvcTrack,
            IHttpContextAccessor httpContextAccessor,
            IOrganisationService organisationService,
            IConfiguration configuration,
            IMapper mapper)
        {
            _logger = logger;
            this.XeroConfig = XeroConfig;
            //_httpClientFactory = httpClientFactory;
            _xeroSvc = xeroSvc;
            _mapper = mapper;
            TokenUtilities.InitializedXeroService(_xeroSvc);
            _apiClientSvc = apiClientSvc;
            _userSvcTrack = userSvcTrack;
            _httpContextAccessor = httpContextAccessor;
            _organisationService = organisationService;
            _configuration = configuration;

        }

        [Route("Authorization/Index/{browser}")]
        public async Task<IActionResult> Index(string browser)
        {
            var client = new XeroClient(XeroConfig.Value);

            var clientState = Guid.NewGuid().ToString();
            TokenUtilities.StoreState(clientState);

            Global.GlobalBrowser = browser;

            //await _userSvcTrack.InsertUserTrackAsync(userTrack);

            return Redirect(client.BuildLoginUri(clientState));
        }

        // GET /Authorization/Callback
        public async Task<ActionResult> Callback(string code, string state)
        {

            if (string.IsNullOrEmpty(code))
            {
                var url = _configuration.GetValue<string>("BatchPaymentAppURL:Url");
                return Redirect(url);
            }
           
            var clientState = TokenUtilities.GetCurrentState();

            if (state != clientState)
            {
                return Content("Cross site forgery attack detected!");
            }

            var client = new XeroClient(XeroConfig.Value);
            var xeroToken = (XeroOAuth2Token)await client.RequestAccessTokenAsync(code);

            if ((xeroToken.IdToken != null) && !JwtUtils.validateIdToken(xeroToken.IdToken, XeroConfig.Value.ClientId))
            {

                return Content("ID token is not valid");
            }

            if ((xeroToken.AccessToken != null) && !JwtUtils.validateAccessToken(xeroToken.AccessToken))
            {
                return Content("Access token is not valid");
            }

            var emailAddress = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "email").Value;
            var givenName = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "given_name").Value;
            var familyName = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "family_name").Value;
            var xeroUserId = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "xero_userid").Value;


            SessionUtility session = new SessionUtility(_httpContextAccessor);
            session.setSession(emailAddress);
            session.setSessionFamilyName(familyName);
            session.setSessionGivenName(givenName);
            session.setSessionXeroUserID(xeroUserId);

            List<Tenant> tenants = await client.GetConnectionsAsync(xeroToken);
            List<XeroUserTenantsSubscriptionModel> xeroUserTenantsSubscriptionModelsList = new List<XeroUserTenantsSubscriptionModel>();


            foreach (var item in tenants)
            {
                var xeroUsertenantsSubscriptionModel = new XeroUserTenantsSubscriptionModel
                {

                    XeroTenant = new XeroTenantModel
                    {
                        XeroTenantUniqueId = item.TenantId.ToString(),
                        Name = item.TenantName,
                    },
                    XeroClient = new XeroClientModel
                    {
                        XeroClientUniqueId = client.xeroConfiguration.ClientId,
                        Name = client.xeroConfiguration.ClientId
                    },
                    SubscriptionPlan = new SubscriptionPlanModel
                    {
                        Id = 1
                    },
                };
                xeroUserTenantsSubscriptionModelsList.Add(xeroUsertenantsSubscriptionModel);
            }

            await _organisationService.UpsertOrganisationAsync(xeroUserTenantsSubscriptionModelsList);

            Tenant firstTenant = tenants[0];

            Global.globallistTenantLog.Add(new TenantLog { TenantId = firstTenant.TenantId, TenantName = firstTenant.TenantName, TenantType = firstTenant.TenantType});

            TokenUtilities.StoreToken(xeroToken, emailAddress);
            TokenUtilities.StoreTenantId(firstTenant.TenantId, emailAddress);


            _xeroSvc.InsertApiLogs(Global.globallistTenantLog[0].TenantId,
                                      Global.globallistTenantLog[0].TenantName,
                                      DateTime.Now,
                                      Global.globallistTenantLog[0].TenantType,
                                      "200", "Get", client.xeroConfiguration.XeroApiBaseUri + "/Connections");


            userTrack = await _apiClientSvc.GetUserIPAsync();
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            userTrack.UserName = emailAddress;
            userTrack.userEmail = emailAddress;//TokenUtilities.EmailAddress;

            userTrack.Page = "Authorization";
            userTrack.methodName = "SignIn";

            await _userSvcTrack.InsertUserTrackAsync(userTrack);

            return RedirectToAction("BatchPayments", "Xero");
        }


        // GET /Authorization/Disconnect
        public async Task<ActionResult> Disconnect()
        {
            var client = new XeroClient(XeroConfig.Value);
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();

            var xeroToken = TokenUtilities.GetStoredToken(emailAddress);
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                emailAddress = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "email").Value;
                //TokenUtilities.EmailAddress = emailAddress;
                //emailAddress = emailAddress;
                TokenUtilities.StoreToken(xeroToken, emailAddress);
            }

            string accessToken = xeroToken.AccessToken;
            Tenant xeroTenant = xeroToken.Tenants[0];

            //await client.DeleteConnectionAsync(xeroToken, xeroTenant);

            //TokenUtilities.DestroyToken(emailAddress);
            session.clearSession();


            return Redirect("https://www.batchpay.com.au/#");
            //return RedirectToAction("Index", "Home");
        }

        public async Task<ActionResult> Revoke()
        {
            var client = new XeroClient(XeroConfig.Value);
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();


            var xeroToken = TokenUtilities.GetStoredToken(emailAddress);
            var utcTimeNow = DateTime.UtcNow;

            if (utcTimeNow > xeroToken.ExpiresAtUtc)
            {
                xeroToken = (XeroOAuth2Token)await client.RefreshAccessTokenAsync(xeroToken);
                emailAddress = JwtUtils.decode(xeroToken.IdToken).Payload.Claims.First(c => c.Type == "email").Value;
                //TokenUtilities.EmailAddress = emailAddress;
                TokenUtilities.StoreToken(xeroToken, emailAddress);
            }

            string accessToken = xeroToken.AccessToken;

            await client.RevokeAccessTokenAsync(xeroToken);

            TokenUtilities.DestroyToken(emailAddress);

            var objUser = await _userSvcTrack.GetDateTimeLog(emailAddress);


            DateTime timeIn = objUser.DateTimeLog;
            DateTime timeOut = DateTime.Now;

            var diffTime = timeOut - timeIn;

            userTrack = await _apiClientSvc.GetUserIPAsync();
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            userTrack.dateTimeLogOut = timeOut;
            userTrack.UserName = emailAddress;
            userTrack.userEmail = emailAddress;//TokenUtilities.EmailAddress;
            userTrack.userUsage = diffTime.Minutes;
            userTrack.Page = "Authorization";
            userTrack.methodName = "SignOut";

            await _userSvcTrack.InsertUserTrackAsync(userTrack);
            session.clearSession();

            return RedirectToAction("Index", "Home");

        }


        [HttpGet]
        public async Task<JsonResult> GetConnectedTenant()
        {
            var client = new XeroClient(XeroConfig.Value);
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var Connections = await client.GetConnectionsAsync(TokenUtilities.GetStoredToken(emailAddress));

            var Data = Connections.ToList<Tenant>();

            var objUser = await _userSvcTrack.GetDateTimeLog(emailAddress);


            DateTime timeIn = objUser.DateTimeLog;
            DateTime timeOut = DateTime.Now;

            var diffTime = timeOut - timeIn;

            userTrack = await _apiClientSvc.GetUserIPAsync();
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            userTrack.dateTimeLogOut = timeOut;
            userTrack.UserName = emailAddress;
            userTrack.userEmail = emailAddress; //TokenUtilities.EmailAddress;
            userTrack.userUsage = diffTime.Minutes;
            userTrack.Page = "Authorization";
            userTrack.methodName = "SignOut";

            await _userSvcTrack.InsertUserTrackAsync(userTrack);
            return Json(Data);
        }


        [HttpGet]
        [Route("Authorization/DisconnectTenant/{tenantId}")]
        public async Task<IActionResult> DisconnectTenant(string tenantId)
        {
            var client = new XeroClient(XeroConfig.Value);
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            var ListOfSelectedTenants = await client.GetConnectionsAsync(TokenUtilities.GetStoredToken(emailAddress));

            var Tenant = ListOfSelectedTenants.Where(t => t.TenantId.ToString().Equals(tenantId)).FirstOrDefault();
            await client.DeleteConnectionAsync(TokenUtilities.GetStoredToken(emailAddress), Tenant);

            var isTenantRemoveFromList = ListOfSelectedTenants.Remove(Tenant);

            if (isTenantRemoveFromList)
            {
                var NewTenant = ListOfSelectedTenants.FirstOrDefault();

                if (NewTenant is null)
                    return RedirectToAction("Disconnect");

                TokenUtilities.StoreTenantId(NewTenant.TenantId, emailAddress);
            }

            var currentTenantID = TokenUtilities.GetCurrentTenantId(emailAddress).ToString();

            userTrack = await _apiClientSvc.GetUserIPAsync();
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            userTrack.UserName = emailAddress;
            userTrack.userEmail = emailAddress;

            userTrack.Page = "Authorization";
            userTrack.methodName = "DisconnectTenant";

            await _userSvcTrack.InsertUserTrackAsync(userTrack);

            return RedirectToAction("Index", "Xero");
        }
    }


}