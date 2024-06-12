namespace XeroApp.Models.BusinessModels
{
    public class XeroUserTenantsSubscriptionModel
    {
        public int Id { get; set; }
        public int XeroClientId { get; set; }
        public int XeroTenantId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public bool IsActive { get; set; }
        public SubscriptionPlanModel SubscriptionPlan { get; set; }
        public XeroClientModel XeroClient { get; set; }
        public XeroTenantModel XeroTenant { get; set; }
    }
}
