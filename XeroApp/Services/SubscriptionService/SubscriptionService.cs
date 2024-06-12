using XeroApp.Models.BusinessModels;
using Persistence.Config;
using AutoMapper;
using Core;


namespace XeroApp.Services.SubscriptionService
{
    public class SubscriptionService : ISubscriptionService
    {

        private readonly XeroAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public SubscriptionService(
            XeroAppDbContext db,
            IMapper mapper,
            IConfiguration config)
        {
            _db = db;
            _mapper = mapper;
            _config = config;
        }

        public Task<XeroUserTenantsSubscriptionModel> InsertTenantSubscriptionAsync(XeroUserTenantsSubscriptionModel model)
        {
            throw new NotImplementedException();
        }

        public async Task<XeroUserTenantsSubscriptionModel> SearchTenantSubscriptionAsync(string tenantUniqueID)
        {
            var tenantSubscriptions = (from tenSubs in _db.XeroUserTenantsSubscription
                                            join tenants in _db.XeroTenant on tenSubs.XeroTenantId equals tenants.Id
                                            where tenants.XeroTenantUniqueId == tenantUniqueID && tenSubs.IsActive == true
                                            select new XeroUserTenantsSubscriptionModel
                                            {
                                                Id = tenSubs.Id,
                                                XeroClientId = tenSubs.XeroClientId,
                                                SubscriptionPlanId = tenSubs.SubscriptionPlanId,    
                                                XeroTenantId = tenSubs.XeroTenantId,
                                                DateStart = tenSubs.DateStart,
                                                DateEnd = tenSubs.DateEnd,
                                                IsActive = tenSubs.IsActive,
                                            }).FirstOrDefault();
            return tenantSubscriptions;
        }
    }
}
