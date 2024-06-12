using XeroApp.Models.BusinessModels;

namespace XeroApp.Services.XeroTenantService
{
    public interface IXeroTenantService
    {
        Task<XeroTenantModel> InsertTenantTrackAsync(XeroTenantModel model);

        Task<XeroTenantModel> SearchTenantAsync(string tenantUniqueID);
    }
}


