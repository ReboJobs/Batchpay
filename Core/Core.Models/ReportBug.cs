
namespace Core
{

    public partial class ReportBug
    {
        public int Id { get; set; }

        public string? Title { get; set; }
        public string? Description { get; set; }

        public DateTime? DateReported { get; set; }

        public string? ReportedBy { get; set; }
        public byte? Status { get; set; }
        public bool? IsActive { get; set; }
        public string ImageBlobURL { get; set; }

        public string? ErrorPath { get; set; }

        public string? StackTrace { get; set; }
    }
}
