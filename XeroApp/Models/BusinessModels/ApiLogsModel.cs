namespace XeroApp.Models.BusinessModels
{
    public class ApiLogsModel
    {

        public int Id { get; set; }
        public string TenantName { get; set; }

        public string TenantType { get; set; }

        public DateTime CreateDateUTC { get; set; }
        public Guid TenantId { get; set; }

        public string url { get; set; }

        public string status { get; set; }

        public string method { get; set; }
    }
}
