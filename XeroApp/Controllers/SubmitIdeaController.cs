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

namespace XeroApp.Controllers
{
    public class SubmitIdeaController: Controller
    {
        private readonly XeroAppDbContext _db;
        private readonly IConfiguration _config;
        private readonly IUserService _userSvc;
        private readonly IXeroService _xeroSvc;
        private readonly IUserTrackService _userSvcTrack;
        private readonly IIdeaService _ideaService;
        private readonly IApiClientService _apiClientSvc;

        private UserTrackModel userTrack = new UserTrackModel();
        private List<IdeaModel> listIdeas { get; set; } = new List<IdeaModel>();

        private readonly IWebHostEnvironment _env;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SubmitIdeaController(
          XeroAppDbContext db,
          IConfiguration config,
          IUserService userSvc,
          IUserTrackService userSvcTrack,
          IIdeaService ideaService,
          IApiClientService apiClientSvc,
          IWebHostEnvironment env,
          IHttpContextAccessor httpContextAccessor
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
        }

        public async Task<IActionResult> Index()
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);
            var emailAddress = session.getSession();
            ViewBag.firstTimeConnection = (emailAddress != null) ? true : false;

            return View();
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
        public async Task<IActionResult> SearchSubmitIdeas([FromBody] IdeaSearchModel searchModel)
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);

            var emailAddress = session.getSession();
            var userName = session.getSessionGivenName() + " " + session.getSessionFamilyName();
            searchModel.UserName = userName;

            String dy = searchModel.DateTo?.Day.ToString();
            String mn = searchModel.DateTo?.Month.ToString();
            String yy = searchModel.DateTo?.Year.ToString();
            String fulldate = dy + "/" + mn + "/" + yy + " 23:59:00";

            searchModel.DateTo = (Convert.ToDateTime(fulldate));

            listIdeas = await _ideaService.SearchSubmitIdeaListAsync(searchModel);
            Global.globallistIdeas = listIdeas;

            userTrack = await _apiClientSvc.GetUserIPAsync();
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            //userTrack.UserName = TokenUtilities.UserName;
            userTrack.userEmail = emailAddress;
            userTrack.Page = "SubmitIdea";
            userTrack.methodName = "SearchSubmitIdeas";

            await _userSvcTrack.InsertUserTrackAsync(userTrack);

            return Json(listIdeas);
        }

        [HttpPost]
        public async Task<JsonResult> Submit([FromBody] IdeaModel ideaModel)
        {
            SessionUtility session = new SessionUtility(_httpContextAccessor);

            var emailAddress = session.getSession();
            var userName = session.getSessionGivenName() + " " + session.getSessionFamilyName();

            ideaModel.SubmittedBy = userName;
            ideaModel.ImageBlobURL = Global.UploadUrl;

            ideaModel = await _ideaService.UpsertIdeasAsync(ideaModel);

            userTrack = await _apiClientSvc.GetUserIPAsync();
            userTrack.Browser = string.Empty;
            userTrack.Browser = Global.GlobalBrowser;
            userTrack.DateTimeLog = DateTime.Now;
            //userTrack.UserName = TokenUtilities.UserName;
            userTrack.userEmail = emailAddress;
            userTrack.Page = "SubmitIdea";
            userTrack.methodName = "SubmitIdea";

            await _userSvcTrack.InsertUserTrackAsync(userTrack);

            return Json(ideaModel);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteIdea(int SubmitIdeaID)
        {

            //int ReportBugIDnumVal = Int32.Parse(ReportBugID);
            SessionUtility session = new SessionUtility(_httpContextAccessor);

            var emailAddress = session.getSession();

            if (SubmitIdeaID > 0)
            {
                await _ideaService.DeleteIdeaAsync(SubmitIdeaID);

                userTrack = await _apiClientSvc.GetUserIPAsync();
                userTrack.Browser = string.Empty;
                userTrack.Browser = Global.GlobalBrowser;
                userTrack.DateTimeLog = DateTime.Now;
                //userTrack.UserName = TokenUtilities.UserName;
                userTrack.userEmail = emailAddress;
                userTrack.Page = "SubmitIdea";
                userTrack.methodName = "DeleteIdea";

                await _userSvcTrack.InsertUserTrackAsync(userTrack);

                //  Send "Success"
                return Json(new { success = true, responseText = "Your message successfuly sent!" });
            }
            else
            {
                //  Send "Fail"
                return Json(new { success = false, responseText = "Invalid submitIdeaID." });
            }

        }

        [HttpPost]
        public async Task<ActionResult> VoteIdea(int votedIdeaID,bool IsUserVoted)
        {

            //int ReportBugIDnumVal = Int32.Parse(ReportBugID);
            SessionUtility session = new SessionUtility(_httpContextAccessor);

            var emailAddress = session.getSession();

            var ideaVoted = Global.globallistIdeas.FirstOrDefault(x => x.Id == votedIdeaID);

            var ideaVoteModel = new IdeaVoteModel
            {
                IdeadId = votedIdeaID,
                VotedBy = emailAddress,
                DateVoted = DateTime.Now,
                IsActive = IsUserVoted
            };


            if (ideaVoted != null)
            {

                if (IsUserVoted)
                {
                    if (!ideaVoted.IsUserVoted)
                    {
                        await _ideaService.VoteIdeaAsync(ideaVoteModel);
                        ideaVoted.IsUserVoted = true;
                        ideaVoted.TotalVotes += 1;
                    }
                }
                else
                {
                    if (ideaVoted.TotalVotes > 0)
                    {
                        await _ideaService.VoteIdeaAsync(ideaVoteModel);
                        ideaVoted.IsUserVoted = false;
                        ideaVoted.TotalVotes -= 1;
                    }
                }
                //listIdeas = listIdeas.ToList();

                userTrack = await _apiClientSvc.GetUserIPAsync();
                userTrack.Browser = string.Empty;
                userTrack.Browser = Global.GlobalBrowser;
                userTrack.DateTimeLog = DateTime.Now;
                //userTrack.UserName = TokenUtilities.UserName;
                userTrack.userEmail = emailAddress;
                userTrack.Page = "SubmitIdea";
                userTrack.methodName = "VoteIdea";

                await _userSvcTrack.InsertUserTrackAsync(userTrack);

                //  Send "Success"
                return Json(new { success = true, responseText = "Your message successfuly sent!" });

            }
            else {

                //  Send "Fail"
                return Json(new { success = false, responseText = "Invalid submitIdeaID." });

            }

        }

    }
}
