
using Core;
using XeroApp.Models.BusinessModels;
using AutoMapper;
using Persistence.Config;
using XeroApp.Enums;
using XeroApp.Models.BusinessModels;
using XeroApp.Services.XeroClientService;
using XeroApp.Services.XeroTenantService;
using Xero.NetStandard.OAuth2.Api;

namespace XeroApp.Services.OrganisationService
{
    public class OrganisationService : IOrganisationService
    {
        private readonly XeroAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;
        private readonly IXeroClientService _xeroClientService;
        private readonly IXeroTenantService _xeroTenantService;

        public OrganisationService(
            XeroAppDbContext db,
            IXeroClientService xeroClientService,
            IXeroTenantService xeroTenantService,
            IMapper mapper,

            IConfiguration config)
        {
            _db = db;
            _mapper = mapper;
            _xeroClientService = xeroClientService;
            _xeroTenantService = xeroTenantService;
            _config = config;

        }


        public async Task UpsertOrganisationAsync(List<XeroUserTenantsSubscriptionModel> model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {

                foreach (var item in model)
                {
                    var xeroClient = await _xeroClientService.SearchClientAsync(item.XeroClient.XeroClientUniqueId); //_db.XeroClient.Where(x => x.XeroClientUniqueId.Equals(item.XeroClient.XeroClientUniqueId)).FirstOrDefault();
                    var xeroTenant = await _xeroTenantService.SearchTenantAsync(item.XeroTenant.XeroTenantUniqueId); //_db.XeroTenant.Where(x => x.XeroTenantUniqueId.Equals(item.XeroTenant.XeroTenantUniqueId)).FirstOrDefault();


                    if (xeroClient == null) {
                        var xeroClientNew = new XeroClientModel();

                        xeroClientNew.XeroClientUniqueId = item.XeroClient.XeroClientUniqueId;
                        xeroClientNew.IsActive = true;
                        xeroClientNew.Name = item.XeroClient.Name;

                        xeroClient = await _xeroClientService.InsertClientTrackAsync(xeroClientNew); //_db.XeroClient.Add(new XeroClientDataModel { XeroClientUniqueId = item.XeroClient.XeroClientUniqueId, Name = item.XeroClient.XeroClientUniqueId, IsActive = true }).Entity;

                    }

                    if (xeroTenant == null) {
                        var xeroTenantNew = new XeroTenantModel();

                        xeroTenantNew.XeroTenantUniqueId = item.XeroTenant.XeroTenantUniqueId;
                        xeroTenantNew.Name = item.XeroTenant.Name;
                        xeroTenantNew.Email = String.Empty;
                        xeroTenantNew.XeroClientId = xeroClient.Id;

                        xeroTenant = await _xeroTenantService.InsertTenantTrackAsync(xeroTenantNew); //_db.XeroTenant.Add(new XeroTenant { XeroTenantUniqueId = item.XeroTenant.XeroTenantUniqueId, Name = item.XeroTenant.Name, Email = String.Empty, Id = xeroClient.Id }).Entity;
                }


                    if (xeroClient != null && xeroTenant != null)
                    {
                        var xeroUserTenantsSubscription = _db.XeroUserTenantsSubscription.Where(x => x.XeroTenantId == xeroTenant.Id && x.XeroClientId == xeroClient.Id).FirstOrDefault();

                        if (xeroUserTenantsSubscription == null)
                            _db.XeroUserTenantsSubscription.Add(new XeroUserTenantsSubscription { XeroClientId = xeroClient.Id, XeroTenantId = xeroTenant.Id, IsActive = true, DateStart = DateTime.Now, DateEnd = DateTime.Now.AddDays(30), SubscriptionPlanId = 1 });

                    }
                    await _db.SaveChangesAsync();
                }
            transaction.Commit();
        }
    }

        public async Task<List<XeroTenantModel>> GetOrganisationByClient(string xeroClientUniqueID)
        {

            var xeroTenantModelList = new List<XeroTenantModel>();

            var result = _db.XeroClient
                .Where(c => c.XeroClientUniqueId.Equals(xeroClientUniqueID))
                .SelectMany(x => x.XeroUserTenantsSubscription.Where(u => u.XeroClientId == x.Id).Select(u => u.XeroTenant).ToList()).ToList();

            result.ForEach(x => xeroTenantModelList.Add(new XeroTenantModel { Id = x.Id, Name = x.Name, XeroTenantUniqueId = x.XeroTenantUniqueId, Email = x.Email }));

            return xeroTenantModelList;
        }


        public async Task<XeroTenantModel> GetCurrentOrganisationByEmail(string emailAddress)
        {

            var currentOrganisation = TokenUtilities.GetCurrentTenantId(emailAddress);
            var result = _db.XeroTenant
                .Where(x => x.XeroTenantUniqueId.Equals(currentOrganisation.ToString()))
                .Select(x => new XeroTenantModel { Id = x.Id, XeroTenantUniqueId = x.XeroTenantUniqueId, Name = x.Name, Email = x.Email }).FirstOrDefault();

            return result;
        }

        public async Task UpdateOrganisationSettingsAsync(XeroTenantModel model)
        {
            using (var transaction = _db.Database.BeginTransaction())
            {
                var xeroTenant = _db.XeroTenant.Where(x => x.Id == model.Id).FirstOrDefault();

                if (xeroTenant != null)
                {
                    xeroTenant.Email = model.Email;
                    await _db.SaveChangesAsync();
                }
                else
                {
                    return;
                }

                transaction.Commit();
            }
        }

        public async Task<List<XeroTenantModel>> GetOrganisationByTenantId(string tenantId, string accessToken)
        {
            var xeroTenantModelList = new List<XeroTenantModel>();
            var accountingApi = new AccountingApi();

            var result = _db.XeroTenant
                .Where(x => x.XeroTenantUniqueId.Equals(tenantId)).ToList();

            var organizationInfo = accountingApi.GetOrganisationsAsync(accessToken, tenantId).Result._Organisations.FirstOrDefault();
           
            //result.Join(organizationInfo ,xt => xt.XeroClientId , o => o)
            result.ForEach(x => xeroTenantModelList.Add(new XeroTenantModel { Id = x.Id, Name = x.Name, XeroTenantUniqueId = x.XeroTenantUniqueId, Email = x.Email , Status = organizationInfo.OrganisationStatus }));

            return xeroTenantModelList;
        }
    }
}
