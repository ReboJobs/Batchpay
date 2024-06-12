

using System.Text.Json.Serialization;

namespace XeroApp.Models.BusinessModels
{
    public class UserTrackModel
    {
        [JsonPropertyName("ip")]
        public string IP { get; set; }

        public int TrackId { get; set; }
        public string UserName { get; set; }

        public string userEmail { get; set; }

        public string Browser { get; set; }

        public DateTime DateTimeLog { get; set; }
        public string Page { get; set; }

        public string methodName { get; set; }


        public DateTime? dateTimeLogOut { get; set; }

        public int userUsage { get; set; } = 0;

        public int TotalNumInvoiceTransaction { get; set; } = 0;


        public int TotalInvoiceTransactionErr { get; set; } = 0;

        public decimal totalInvoiceAmount { get; set; } = 0;


    }
}
