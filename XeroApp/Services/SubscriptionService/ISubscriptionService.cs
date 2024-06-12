using Core;
using XeroApp.Models.BusinessModels;

namespace XeroApp.Services.SubscriptionService
{
    public interface ISubscriptionService
    {
        Task<XeroUserTenantsSubscriptionModel> InsertTenantSubscriptionAsync(XeroUserTenantsSubscriptionModel model);

        Task<XeroUserTenantsSubscriptionModel> SearchTenantSubscriptionAsync(string tenantUniqueID);
    }
}
