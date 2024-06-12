using Core;
using XeroApp.Models.BusinessModels;
using AutoMapper;
using Persistence.Config;

namespace XeroApp.Services.UserTrackService
{
    public class UserTrackService: IUserTrackService
    {

        private readonly XeroAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public UserTrackService(
            XeroAppDbContext db,
            IMapper mapper,
            IConfiguration config)
        {
            _db = db;
            _mapper = mapper;
            _config = config;
        }

        public async Task<UserTrackModel> GetDateTimeLog(string userName)
        {
            var qry = (from u in _db.UserTrack
                       where (u.UserName == userName) && (u.methodName == "SignIn")
                       orderby u.TrackId descending
                       select new UserTrackModel
                       {
                           TrackId = u.TrackId,
                           UserName = u.UserName,
                           DateTimeLog = u.DateTimeLog,
                           Browser = u.Browser
                       }).FirstOrDefault();

            return qry;
        }


        public async Task InsertUserTrackAsync(UserTrackModel model)
        {

            var useTrackDb = new UserTrack
            {
                Ip = model.IP,
                UserName = model.UserName,
                UserEmail = model.userEmail,
                DateTimeLog = model.DateTimeLog,
                Browser = model.Browser,
                Page = model.Page,
                methodName = model.methodName,
                userUsage = model.userUsage,
                TotalInvoiceTransactionErr = model.TotalInvoiceTransactionErr,
                TotalNumInvoiceTransaction = model.TotalNumInvoiceTransaction,
                totalInvoiceAmount = model.totalInvoiceAmount,
                dateTimeLogOut = model.dateTimeLogOut,
            };

            try
            {
                _db.UserTrack.Add(useTrackDb);
                await _db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                string msg = ex.Message;
            }

        }
    }
}
