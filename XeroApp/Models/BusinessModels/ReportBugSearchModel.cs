using XeroApp.Enums;

namespace XeroApp.Models.BusinessModels
{
    public class ReportBugSearchModel
    {
        public string Title { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public EnumReportBugStatus ReportBugStatus { get; set; }
        public string ReportedBy { get; set; }
    }
}
