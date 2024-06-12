using Core;

using XeroApp.Models.BusinessModels;

namespace XeroApp.Services.ApiClientService
{
    public interface IApiClientService
    {
        Task<UserTrackModel> GetUserIPAsync();
    }
}
