


namespace Core
{
    public partial class UserTrack
    {

        public int TrackId { get; set; }

        public string? Ip { get; set; }

        public string? UserName { get; set; }

        public DateTime DateTimeLog { get; set; }

        public string? Browser { get; set; }

        public string? Page { get; set; }

        public string? methodName { get; set; }

        public string? UserEmail { get; set; }

        public DateTime?  dateTimeLogOut { get; set; }

        public int userUsage { get; set; }

        public int TotalNumInvoiceTransaction { get; set; }


        public int TotalInvoiceTransactionErr { get; set; }

        public decimal totalInvoiceAmount { get; set; } 

    }
}
