
using XeroApp.Models.BusinessModels;

namespace XeroApp.Services.UserTrackService
{
    public interface IUserTrackService
    {
        Task InsertUserTrackAsync(UserTrackModel model);

        Task<UserTrackModel> GetDateTimeLog(string userName);
    }
}

