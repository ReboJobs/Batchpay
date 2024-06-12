
using XeroApp.Models.BusinessModels;
using XeroApp.Services.UserTrackService;
using Persistence.Config;
using AutoMapper;
using Core;

namespace XeroApp.Services.XeroTenantService
{
    public class XeroTenantService: IXeroTenantService
    {
        private readonly XeroAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public XeroTenantService(
            XeroAppDbContext db,
            IMapper mapper,
            IConfiguration config)
        {
            _db = db;
            _mapper = mapper;
            _config = config;
        }

        public async Task<XeroTenantModel> InsertTenantTrackAsync(XeroTenantModel model)
        {
            var tenantDB = _db.XeroTenant.FirstOrDefault(x => x.XeroTenantUniqueId == model.XeroTenantUniqueId);

            if (tenantDB != null)
            {
                // NOT Implement yet
            }
            else
            {
                tenantDB = new XeroTenant()
                {
                    XeroTenantUniqueId = model.XeroTenantUniqueId,
                    Name = model.Name,
                    Email = model.Email,
                    XeroClientId = model.XeroClientId
                };

                _db.XeroTenant.Add(tenantDB);
            }

            await _db.SaveChangesAsync();
            model.Id = tenantDB.Id;

            return model;
        }

        public async Task<XeroTenantModel> SearchTenantAsync(string tenantUniqueID)
        {
            var tenant = _db.XeroTenant.Where(x => x.XeroTenantUniqueId.Equals(tenantUniqueID))
                        .Select(x => new XeroTenantModel
                        {
                            Id = x.Id,
                            XeroTenantUniqueId = x.XeroTenantUniqueId,
                            Name = x.Name,
                            Email = x.Email,
                            XeroClientId = x.XeroClientId,
                        }).FirstOrDefault();

            return tenant;

        }
    }
}





