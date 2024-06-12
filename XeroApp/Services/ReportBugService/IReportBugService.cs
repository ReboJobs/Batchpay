
using XeroApp.Models.BusinessModels;

namespace XeroApp.Services.ReportBugService
{
    public interface IReportBugService
    {
        Task<ReportBugModel> UpsertReportBugAsync(ReportBugModel model);
        Task<List<ReportBugModel>> GestReportBugListAsync(ReportBugSearchModel model);
        Task DeleteReportBugAsync(int reportBugId);
        Task UpsertReportBugImageBlobUrlAsync(int ideaId, string imageUrl);

    }
}
