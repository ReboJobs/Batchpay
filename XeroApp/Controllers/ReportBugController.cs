using Microsoft.AspNetCore.Mvc;
using XeroApp.Models.BusinessModels;
using Persistence.Config;
using XeroApp.Services;
using XeroApp.Services.ApiClientService;
using XeroApp.Services.UserTrackService;
using XeroApp.Services.ReportBugService;
using XeroApp.Utilities;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using XeroApp.Enums;
using Microsoft.AspNetCore.Diagnostics;
using XeroApp.Models;
using XeroApp.Extensions;

namespace XeroApp.Controllers
{

    public class ReportBugController : Controller
    {
        private readonly XeroAppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IUserService _userSvc;
        private readonly IXeroService _xeroSvc;
        private readonly IUserTrackService _userSvcTrack;
        private readonly IReportBugService _reportBugService;
        private readonly IApiClientService _apiClientSvc;

        private UserTrackModel userTrack = new UserTrackModel();
        private List<ReportBugModel> listReportBugs { get; set; } = new List<ReportBugModel>();
        private ReportBugModel reportBugModel { get; set; } = new ReportBugModel();

        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ReportBugController(
           XeroAppDbContext db,
           IConfiguration config,
           IUserService userSvc,
           IUserTrackService userSvcTrack,
           IReportBugService reportBugService,
           IApiClientService apiClientSvc,
           IWebHostEnvironment env,
           IHttpContextAccessor httpContextAccessor
           )
        {
            _db = db;
            _config = config;
            _userSvc = userSvc;
            _userSvcTrack = userSvcTrack;
            _reportBugService = reportBugService;
            _apiClientSvc = apiClientSvc;
            _env = env;
            _httpContextAccessor = httpContextAccessor;
        }


        public async Task<IActionResult> Index()
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;

            if (!TokenUtilities.TokenExists(emailAddress) || emailAddress == null)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ErrorDisplayJson([FromBody] ErrorDisplayViewModel errorDisplayViewModel)
        {
            string result = this.RenderViewAsync("ErrorDisplayJson", errorDisplayViewModel, true).Result;
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> ErrorDisplayJsonReauthorized([FromBody] ErrorDisplayViewModel errorDisplayViewModel)
        {
            
            if (errorDisplayViewModel.errorCode == 401)
            {

                errorDisplayViewModel.exceptionPath = "Unauthorized";
                errorDisplayViewModel.exceptionMessage = "User must reauthorized";
            }
            if (errorDisplayViewModel.errorCode == 503)
            {
                errorDisplayViewModel.exceptionPath = "Organisation offline";
                errorDisplayViewModel.exceptionMessage = "User must reauthorized";
            }

            if (errorDisplayViewModel.errorCode == 429)
            {
                errorDisplayViewModel.exceptionPath = "Rate Limit Exceeded - The API rate limit for your organisation/application pairing has been exceeded";
                errorDisplayViewModel.exceptionMessage = "Please wait for a few minutes before trying again";
            }

            string result = this.RenderViewAsync("ErrorDisplayJsonReauthorized", errorDisplayViewModel, true).Result;
            return Json(result);
        }

        [HttpPost]
        public async Task<IActionResult> UploadImage()
        {
            byte[] bytes = null;

            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();


            foreach (var formFile in Request.Form.Files)
            {
                var fulPath = Path.Combine(_env.ContentRootPath, "wwwroot\\files", formFile.FileName);

                using (var ms = new MemoryStream())
                {
                    formFile.CopyTo(ms);
                    bytes = ms.ToArray();
                }

                CloudStorageAccount cloudStorageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=storageaccountlanz9402;AccountKey=w/99ADayKZqqXlj2oP7g/RJUGMadxsKh/KUW8WQr1SQypKKqRNn5pXANZMQuuzj4BqEOhX98Ga8idLP45mnNhQ==;EndpointSuffix=core.windows.net");
                var blobClient = cloudStorageAccount.CreateCloudBlobClient();
                var container = blobClient.GetContainerReference("images");
                await container.CreateIfNotExistsAsync();
                await container.SetPermissionsAsync(new BlobContainerPermissions()
                {
                    PublicAccess = BlobContainerPublicAccessType.Blob
                });
                string Filename = this.GenerateFileName(formFile.FileName, emailAddress);
                var blob = container.GetBlockBlobReference(Filename);
                blob.Properties.ContentType = formFile.ContentType;
                //byte[] bytes = System.IO.File.ReadAllBytes(fulPath);
                await blob.UploadFromByteArrayAsync(bytes, 0, bytes.Length);
                Global.UploadUrl = blob.Uri.AbsoluteUri;


                return Json("Upload file succesfully.");
            }
            return Json("Please Try Again !!");
        }

        private string GenerateFileName(string fileName, string subDirectory)
        {
            string strFileName = string.Empty;
            string[] strName = fileName.Split('.');
            strFileName = subDirectory + "/" + DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmssfff") + "." + strName[strName.Length - 1];
            return strFileName;
        }

        [HttpPost]
        public async Task<IActionResult> GetReportBugs([FromBody] ReportBugSearchModel searchModel)
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);

            var emailAddress = session.getSession();
            searchModel.ReportedBy = emailAddress;

            //searchModel.ReportedBy = TokenUtilities.EmailAddress;

            listReportBugs = await _reportBugService.GestReportBugListAsync(searchModel);

            userTrack = await _apiClientSvc.GetUserIPAsync();
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            //userTrack.UserName = TokenUtilities.UserName;
            userTrack.userEmail = emailAddress;
            userTrack.Page = "ReportBug";
            userTrack.methodName = "GetReportBugs";

            await _userSvcTrack.InsertUserTrackAsync(userTrack);

            return Json(listReportBugs);
        }


        [HttpPost]
        public async Task<JsonResult> SubmitBug([FromBody] ReportBugModel reportBugModel)
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);

            var emailAddress = session.getSession();

            reportBugModel.ReportedBy = emailAddress;
            reportBugModel.ImageBlobUrl = Global.UploadUrl;

            reportBugModel = await _reportBugService.UpsertReportBugAsync(reportBugModel);

            userTrack = await _apiClientSvc.GetUserIPAsync();
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            //userTrack.UserName = TokenUtilities.UserName;
            userTrack.userEmail = emailAddress;
            userTrack.Page = "ReportBug";
            userTrack.methodName = "SubmitBug";

            await _userSvcTrack.InsertUserTrackAsync(userTrack);

            return Json(reportBugModel);
        }



        [HttpPost]
        public async Task<ActionResult> DeleteBug(int ReportBugID)
        {

            //int ReportBugIDnumVal = Int32.Parse(ReportBugID);
            SessionUtility session = new SessionUtility(_httpContextAccessor);

            var emailAddress = session.getSession();

            if (ReportBugID > 0)
            {
                await _reportBugService.DeleteReportBugAsync(ReportBugID);

                userTrack = await _apiClientSvc.GetUserIPAsync();
                userTrack.Browser = string.Empty;
                userTrack.Browser = Global.GlobalBrowser;
                userTrack.DateTimeLog = DateTime.Now;
                //userTrack.UserName = TokenUtilities.UserName;
                userTrack.userEmail = emailAddress;
                userTrack.Page = "ReportBug";
                userTrack.methodName = "DeleteBug";

                await _userSvcTrack.InsertUserTrackAsync(userTrack);

                //  Send "Success"
                return Json(new { success = true, responseText = "Your message successfuly sent!" });
            }
            else
            {
                //  Send "Fail"
                return Json(new { success = false, responseText = "Invalid reportbugID." });
            }

        }





    }
}
