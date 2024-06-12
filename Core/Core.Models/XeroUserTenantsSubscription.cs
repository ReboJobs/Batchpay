using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Core
{
    public class XeroUserTenantsSubscription
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int XeroClientId { get; set; }
        public int XeroTenantId { get; set; }
        public int SubscriptionPlanId { get; set; }
        public DateTime DateStart { get; set; }
        public DateTime DateEnd { get; set; }
        public bool IsActive { get; set; }
        public SubscriptionPlan SubscriptionPlan { get; set; }
        public XeroClientDataModel XeroClient { get; set; }
        public XeroTenant XeroTenant { get; set; }



    }
}
