
using XeroApp.Models.BusinessModels;
using Persistence.Config;
using Core;


namespace XeroApp.Services.ApiLogsService
{
    public class ApiLogsService : IApiLogsService
    {

		private readonly XeroAppDbContext _db;
		private readonly IConfiguration _config;

		public ApiLogsService(
		XeroAppDbContext db,
		IConfiguration config)
		{
			_db = db;
			_config = config;
		}
		public async Task<List<ApiLogsModel>> SearchApiLogsListAsync(ApiLogsModel model)
		{
			var apiLogsModel = new List<ApiLogsModel>();

			if (model.TenantName == null || model.TenantName == "")
			{
				apiLogsModel = _db.XeroApiLogs
				.Select(x => new ApiLogsModel
				{
					Id = x.Id,
					TenantId = x.TenantId,
					TenantName = x.TenantName,
					CreateDateUTC = x.CreateDateUtc,
					TenantType = x.Type,
					url = x.Url,
					status = x.Status,
					method = x.Method,
				}).OrderByDescending(x => x.CreateDateUTC).ToList();

			}
			else {

				apiLogsModel = _db.XeroApiLogs
				.Where(s => (s.TenantName.Contains(model.TenantName) || string.IsNullOrEmpty(model.TenantName)))
				.Select(x => new ApiLogsModel
				{
					Id = x.Id,
					TenantId = x.TenantId,
					TenantName = x.TenantName,
					CreateDateUTC = x.CreateDateUtc,
					TenantType = x.Type,
					url = x.Url,
					status = x.Status,
					method = x.Method,
				}).OrderByDescending(x => x.CreateDateUTC).ToList();

			}
		
			return apiLogsModel;
		}

        public async Task<ApiLogsModel> UpsertApiLogsAsync(ApiLogsModel model)
		{

			var apiLogDb = new XeroApiLog
			{
				TenantId = model.TenantId,
				TenantName = model.TenantName,
				CreateDateUtc = model.CreateDateUTC,
				Type = model.TenantType,
				Url = model.url,
				Status = model.status,
				Method = model.method,
			};

			_db.XeroApiLogs.Add(apiLogDb);


			await _db.SaveChangesAsync();
			model.Id = apiLogDb.Id;

			return model;
		}

      
    }

}
