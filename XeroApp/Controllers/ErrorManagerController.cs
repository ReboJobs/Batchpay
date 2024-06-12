using Microsoft.AspNetCore.Mvc;
using XeroApp.Models.BusinessModels;
using Persistence.Config;
using XeroApp.Services;
using XeroApp.Services.ApiClientService;
using XeroApp.Services.UserTrackService;
using XeroApp.Services.ReportBugService;
using XeroApp.Services.IdeaService;
using XeroApp.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using XeroApp.Enums;
using Microsoft.AspNetCore.Diagnostics;

namespace XeroApp.Controllers
{
    public class ErrorManagerController : Controller
    {
        private readonly XeroAppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IUserService _userSvc;
        private readonly IXeroService _xeroSvc;
        private readonly IUserTrackService _userSvcTrack;
        private readonly IIdeaService _ideaService;
        private readonly IApiClientService _apiClientSvc;
        private readonly IReportBugService _reportBugService;

        private UserTrackModel userTrack = new UserTrackModel();
        private List<IdeaModel> listIdeas { get; set; } = new List<IdeaModel>();

        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ErrorManagerController(
          XeroAppDbContext db,
          IConfiguration config,
          IUserService userSvc,
          IUserTrackService userSvcTrack,
          IIdeaService ideaService,
          IApiClientService apiClientSvc,
          IWebHostEnvironment env,
          IHttpContextAccessor httpContextAccessor,
           IReportBugService reportBugService
          )
        {
            _db = db;
            _config = config;
            _userSvc = userSvc;
            _userSvcTrack = userSvcTrack;
            _ideaService = ideaService;
            _apiClientSvc = apiClientSvc;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
            _reportBugService = reportBugService;
        }

        public async Task<IActionResult> Index()
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            ReportBugModel reportBugModel = new ReportBugModel();
            ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;
            var exceptionHandlerPathFeature = _httpContextAccessor.HttpContext.Features.Get<IExceptionHandlerPathFeature>();


            var result = exceptionHandlerPathFeature.Error.InnerException as Xero.NetStandard.OAuth2.Client.ApiException;
            if (result != null)
            {
                var ErrorCode = ((Xero.NetStandard.OAuth2.Client.ApiException)exceptionHandlerPathFeature.Error.InnerException).ErrorCode;

                if (ErrorCode == 401)
                {
                    ViewBag.XeroResponse = "Unauthorized";
                    ViewBag.XeroResponseMessage = "User must reauthorized";
                    return View("XeroErrorReauthorized");
                }
                if (ErrorCode == 503)
                {
                    ViewBag.XeroResponse = "Organisation offline";
                    ViewBag.XeroResponseMessage = "User must reauthorized";
                    return View("XeroErrorReauthorized");
                }
            }

            ViewBag.exceptionPath = exceptionHandlerPathFeature.Path;
            ViewBag.exceptionMessage = exceptionHandlerPathFeature.Error.Message;
            ViewBag.stacktrace = exceptionHandlerPathFeature.Error.StackTrace;
            reportBugModel.ReportedBy = emailAddress;

            reportBugModel.Title = exceptionHandlerPathFeature.Path;
            reportBugModel.ReportBugStatus = Enums.EnumReportBugStatus.Open;
            reportBugModel.Description = exceptionHandlerPathFeature.Error.Message;
            reportBugModel.StackTrace = exceptionHandlerPathFeature.Error.StackTrace;
            reportBugModel.ErrorPath =  exceptionHandlerPathFeature.Path;
            await _reportBugService.UpsertReportBugAsync(reportBugModel);
            return View();
        }

    }
}
