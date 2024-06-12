using XeroApp.Models.BusinessModels;

namespace XeroApp.Services.IdeaService
{
    public interface IIdeaService
    {

        Task<IdeaModel> UpsertIdeasAsync(IdeaModel model);
        Task<List<IdeaModel>> SearchSubmitIdeaListAsync(IdeaSearchModel model);
        Task DeleteIdeaAsync(int ideaId);
        Task<IdeaVoteModel> VoteIdeaAsync(IdeaVoteModel model);
        Task UpsertIdeasImageBlobUrlAsync(int ideaId, string imageUrl);

    }
}
