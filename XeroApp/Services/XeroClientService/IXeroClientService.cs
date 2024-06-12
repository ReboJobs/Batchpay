using Core;
using XeroApp.Models.BusinessModels;

namespace XeroApp.Services.XeroClientService
{
    public interface IXeroClientService
    {
        Task<XeroClientModel> InsertClientTrackAsync(XeroClientModel model);

        Task<XeroClientModel> SearchClientAsync(string clientUniqueID);
    }
}


