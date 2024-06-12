using Core;
using XeroApp.Models.BusinessModels;

namespace XeroApp.Services.OrganisationService
{
    public interface IOrganisationService
    {
        Task UpsertOrganisationAsync(List<XeroUserTenantsSubscriptionModel> model);

        Task<List<XeroTenantModel>> GetOrganisationByClient(string xeroClientUniqueID);

        Task<List<XeroTenantModel>> GetOrganisationByTenantId(string tenantId, string accessToken);

        Task UpdateOrganisationSettingsAsync(XeroTenantModel model);

        Task<XeroTenantModel> GetCurrentOrganisationByEmail(string emailAddress);

    }
}
