using XeroApp.Models.BusinessModels;
using Persistence.Config;
using Core;

namespace XeroApp.Services.IdeaService
{
	public class IdeaService : IIdeaService
	{
		private readonly XeroAppDbContext _db;
		private readonly IConfiguration _config;

		public IdeaService(
		XeroAppDbContext db,
		IConfiguration config)
		{
			_db = db;
			_config = config;
		}

		public async Task<IdeaModel> UpsertIdeasAsync(IdeaModel model)
		{
			var ideaDb = _db.Ideas.FirstOrDefault(x => x.Id == model.Id);

			if (ideaDb != null)
			{
				ideaDb.Title = model.Title;
				ideaDb.Description = model.Description;
				ideaDb.ImageBlobURL = model.ImageBlobURL;
			}
			else
			{
				ideaDb = new Idea
				{
					Title = model.Title,
					Description = model.Description,
					DateCreated = DateTime.Now,
					SubmittedBy = model.SubmittedBy,
					IsActive = true,
					ImageBlobURL = model.ImageBlobURL,
				};

				_db.Ideas.Add(ideaDb);
			}

			await _db.SaveChangesAsync();
			model.Id = ideaDb.Id;

			return model;
		}
		public async Task UpsertIdeasImageBlobUrlAsync(int ideaId, string imageUrl)
		{
			var ideaDb = _db.Ideas.FirstOrDefault(x => x.Id == ideaId);

			if (ideaDb != null)
			{
				ideaDb.ImageBlobURL = imageUrl;
			}

			await _db.SaveChangesAsync();
		}

		public async Task<IdeaVoteModel> VoteIdeaAsync(IdeaVoteModel model)
		{
			try
			{

				var ideaVoteDb = _db.IdeaVoteDetails.FirstOrDefault(x => x.IdeaId == model.IdeadId && x.VotedBy == model.VotedBy);

				if (ideaVoteDb != null)
				{
					ideaVoteDb.IsActive = model.IsActive;
				}
				else
				{
					ideaVoteDb = new IdeaVoteDetail
					{
						VotedBy = model.VotedBy,
						IdeaId = model.IdeadId,
						DateVoted = model.DateVoted,
						IsActive = model.IsActive,
					};

					_db.IdeaVoteDetails.Add(ideaVoteDb);
				}

				await _db.SaveChangesAsync();
				model.Id = ideaVoteDb.Id;

				return model;

			}
			catch (Exception ex)
			{
				return null;
			}

		}

		public async Task<List<IdeaModel>> SearchSubmitIdeaListAsync(IdeaSearchModel model)
		{
			var ideaModel = _db.Ideas
			.Where(s => (s.DateCreated >= model.DateFrom && s.DateCreated <= model.DateTo)
					 && s.IsActive == true
					 && (s.Title.Contains(model.Text) || s.Description.Contains(model.Text) || string.IsNullOrEmpty(model.Text)))
			.Select(x => new IdeaModel
			{
				Id = x.Id,
				Title = x.Title ?? String.Empty,
				Description = x.Description ?? String.Empty,
				DateCreated = x.DateCreated,
				SubmittedBy = x.SubmittedBy ?? String.Empty,
				//IsUserVoted = _db.IdeaVoteDetails.FirstOrDefault(i => i.IdeaId == x.Id && i.VotedBy == model.UserName && x.IsActive == true) != null,
				//TotalVotes = _db.IdeaVoteDetails.Where(i => i.IdeaId == x.Id && x.IsActive == true).Count(),
				IsActive = x.IsActive,
				ImageBlobURL = x.ImageBlobURL ?? String.Empty
			}).OrderByDescending(x => x.DateCreated).ToList();

			foreach (var idea in ideaModel) {
				idea.IsUserVoted = _db.IdeaVoteDetails.FirstOrDefault(i => i.IdeaId == idea.Id && i.VotedBy == model.UserName && i.IsActive == true) != null;
				idea.TotalVotes = _db.IdeaVoteDetails.Where(i => i.IdeaId == idea.Id && i.IsActive == true).Count();
			}
			
			return ideaModel;
		}

		public async Task DeleteIdeaAsync(int ideaId)
		{
			var ideaDb = _db.Ideas.FirstOrDefault(x => x.Id == ideaId);

			if (ideaDb != null)
			{
				ideaDb.IsActive = false;
				await _db.SaveChangesAsync();
			}
		}

	}
}
