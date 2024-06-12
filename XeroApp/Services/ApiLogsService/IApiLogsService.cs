using XeroApp.Models.BusinessModels;

namespace XeroApp.Services.ApiLogsService
{
    public interface IApiLogsService
    {
        Task<ApiLogsModel> UpsertApiLogsAsync(ApiLogsModel model);
        Task<List<ApiLogsModel>> SearchApiLogsListAsync(ApiLogsModel model);
    }
}
