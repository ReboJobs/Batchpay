
using Core;
using XeroApp.Models.BusinessModels;
using AutoMapper;
using Persistence.Config;
using XeroApp.Enums;

namespace XeroApp.Services.ReportBugService
{
	public class ReportBugService : IReportBugService
	{
		private readonly XeroAppDbContext _db;
		private readonly IMapper _mapper;
		private readonly IConfiguration _config;

		public ReportBugService(
			XeroAppDbContext db,
			IMapper mapper,
			IConfiguration config)
		{
			_db = db;
			_mapper = mapper;
			_config = config;
		}

		public async Task<ReportBugModel> UpsertReportBugAsync(ReportBugModel model)
		{
			var reportBugDb = _db.ReportBug.FirstOrDefault(x => x.Id == model.Id);

			if (reportBugDb != null)
			{
				reportBugDb.Title = model.Title;
				reportBugDb.Description = model.Description;
				reportBugDb.Status = (byte)model.ReportBugStatus;
				reportBugDb.ImageBlobURL = model.ImageBlobUrl;
				reportBugDb.ErrorPath = model.ErrorPath;
				reportBugDb.StackTrace = model.StackTrace;

			}
			else
			{
				reportBugDb = new ReportBug
				{
					Title = model.Title,
					Description = model.Description,
					DateReported = DateTime.Now,
					ReportedBy = model.ReportedBy,
					Status = (byte)model.ReportBugStatus,
					IsActive = true,
					ImageBlobURL = model.ImageBlobUrl,
					ErrorPath = model.ErrorPath,
					StackTrace = model.StackTrace
				};

				_db.ReportBug.Add(reportBugDb);
			}

			await _db.SaveChangesAsync();
			model.Id = reportBugDb.Id;

			return model;
		}


		public async Task<List<ReportBugModel>> GestReportBugListAsync(ReportBugSearchModel model)
		{
			try
			{
				var reportBugModel = _db.ReportBug
				.Where(s => (s.DateReported >= model.DateFrom && s.DateReported <= model.DateTo)
						 && s.IsActive == true
						 && (s.Title.Contains(model.Title) || s.Description.Contains(model.Title) || string.IsNullOrEmpty(model.Title))
						 && (s.Status == (byte)model.ReportBugStatus || (byte)model.ReportBugStatus == 0)
						 && s.ReportedBy == model.ReportedBy || string.IsNullOrEmpty(model.ReportedBy
						 ))
				.Select(x => new ReportBugModel
				{
					Id = x.Id,
					Title = x.Title,
					Description = x.Description,
					DateReported = x.DateReported,
					ReportedBy = x.ReportedBy,
					ReportBugStatus = (EnumReportBugStatus)x.Status,
					IsActive = x.IsActive,
					ImageBlobUrl = x.ImageBlobURL ?? String.Empty
				}).OrderByDescending(x => x.DateReported).ToList();

				return reportBugModel;

			}
			catch (Exception ex)
			{
				string msg = ex.Message;
			}

			return null;
		}

		public async Task DeleteReportBugAsync(int reportBugId)
		{

			var reportBugDb = _db.ReportBug.FirstOrDefault(x => x.Id == reportBugId);

			if (reportBugDb != null)
			{
				reportBugDb.IsActive = false;
				await _db.SaveChangesAsync();
			}

		}
		public async Task UpsertReportBugImageBlobUrlAsync(int ideaId, string imageUrl)
		{
			var reportBugDb = _db.ReportBug.FirstOrDefault(x => x.Id == ideaId);

			if (reportBugDb != null)
			{
				reportBugDb.ImageBlobURL = imageUrl;
			}

			await _db.SaveChangesAsync();
		}

	}
}
