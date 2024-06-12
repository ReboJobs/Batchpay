namespace XeroApp.Models.BusinessModels
{
    public class XeroClientModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string XeroClientUniqueId { get; set; }
        public bool IsActive { get; set; }
        public ICollection<XeroUserTenantsSubscriptionModel> XeroUserTenantsSubscription { get; set; }
    }
}
