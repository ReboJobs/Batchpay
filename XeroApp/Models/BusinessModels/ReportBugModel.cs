using XeroApp.Enums;

namespace XeroApp.Models.BusinessModels
{
    public class ReportBugModel
    {
        public int Id { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }

        public DateTime? DateReported { get; set; }

        public string? ReportedBy { get; set; }
        public EnumReportBugStatus ReportBugStatus { get; set; }
        public bool? IsActive { get; set; }

        public string ImageBlobUrl { get; set; }

        public string? ErrorPath { get; set; }

        public string? StackTrace { get; set; }

        public string ReportBugStatusName
        {
            get
            {

                return ((Enum)Enum.Parse(typeof(EnumReportBugStatus), ReportBugStatus.ToString())).GetDescription();
            }
        }

    }
}

