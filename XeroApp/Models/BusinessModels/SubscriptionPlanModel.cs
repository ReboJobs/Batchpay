namespace XeroApp.Models.BusinessModels
{
    public class SubscriptionPlanModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public ICollection<XeroUserTenantsSubscriptionModel> XeroUserTenantsSubscription { get; set; }
    }
}
