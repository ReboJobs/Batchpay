using Microsoft.AspNetCore.Mvc;
using XeroApp.Utilities;
using XeroApp.Models.BusinessModels;
using XeroApp.Services.ApiClientService;
using XeroApp.Services.ApiLogsService;
using XeroApp.Services.UserTrackService;

namespace XeroApp.Controllers
{
    public class XeroApiLogsController : Controller
    {

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IApiClientService _apiClientSvc;
        private readonly IApiLogsService _apiLogService;
        private readonly IUserTrackService _userSvcTrack;

        private List<ApiLogsModel> listXeroApiLogs { get; set; } = new List<ApiLogsModel>();

        private UserTrackModel userTrack = new UserTrackModel();


        public XeroApiLogsController(
           IHttpContextAccessor httpContextAccessor,
           IApiClientService apiClientSvc,
           IApiLogsService apiLogService,
           IUserTrackService userTrackService
        )

        {
            _httpContextAccessor = httpContextAccessor;
            _apiClientSvc = apiClientSvc;
            _apiLogService = apiLogService;
            _userSvcTrack = userTrackService;
        }

        public IActionResult Index()
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();

            ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;

            return View();
        }

        public async Task<IActionResult> SearchXeroApiLogs([FromBody] ApiLogsModel searchModel)
        {

            SessionUtility session = new SessionUtility(_httpContextAccessor);

            var emailAddress = session.getSession();

            listXeroApiLogs = await _apiLogService.SearchApiLogsListAsync(searchModel);

            userTrack = await _apiClientSvc.GetUserIPAsync();
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            userTrack.userEmail = emailAddress;
            userTrack.Page = "XeroApiLogs";
            userTrack.methodName = "SearchXeroApiLogs";

            await _userSvcTrack.InsertUserTrackAsync(userTrack);

            return Json(listXeroApiLogs);
        }
    }
}
