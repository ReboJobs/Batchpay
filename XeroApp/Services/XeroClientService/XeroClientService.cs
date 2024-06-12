using XeroApp.Models.BusinessModels;
using Persistence.Config;
using AutoMapper;
using Core;


namespace XeroApp.Services.XeroClientService
{
    public class XeroClientService : IXeroClientService
    {
        private readonly XeroAppDbContext _db;
        private readonly IMapper _mapper;
        private readonly IConfiguration _config;

        public XeroClientService(
            XeroAppDbContext db,
            IMapper mapper,
            IConfiguration config)
        {
            _db = db;
            _mapper = mapper;
            _config = config;
        }

        public async Task<XeroClientModel> InsertClientTrackAsync(XeroClientModel model)
        {
            var clientDB = _db.XeroClient.FirstOrDefault(x => x.XeroClientUniqueId == model.XeroClientUniqueId);

            if (clientDB != null)
            {
                // NOT Implement yet
            }
            else
            {
                clientDB = new XeroClientDataModel
                {
                   XeroClientUniqueId = model.XeroClientUniqueId,
                   IsActive = model.IsActive,
                   Name = model.XeroClientUniqueId,
                };

                _db.XeroClient.Add(clientDB);
            }

            await _db.SaveChangesAsync();
            model.Id = clientDB.Id;

            return model;
        }


        public async Task<XeroClientModel> SearchClientAsync(string clientUniqueID)
        {
            var tenant = _db.XeroClient.Where(x => x.XeroClientUniqueId.Equals(clientUniqueID))
                        .Select(x => new XeroClientModel
                        {
                            Id = x.Id,
                            XeroClientUniqueId = x.XeroClientUniqueId,
                            Name = x.XeroClientUniqueId,
                            IsActive = x.IsActive,
                        }).FirstOrDefault();

            return tenant;

        }

    }
}
