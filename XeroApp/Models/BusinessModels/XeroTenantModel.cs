namespace XeroApp.Models.BusinessModels
{
    public class XeroTenantModel
    {
        public int Id { get; set; }
        public string XeroTenantUniqueId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public int? XeroClientId { get; set; }

        public string Status { get; set; }
        public ICollection<XeroUserTenantsSubscriptionModel> XeroUserTenantsSubscription { get; set; }
    }
}
