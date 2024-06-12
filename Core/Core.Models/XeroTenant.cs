using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    public class XeroTenant
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string XeroTenantUniqueId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }

        public int? XeroClientId { get; set; }
        public ICollection<XeroUserTenantsSubscription> XeroUserTenantsSubscription { get; set; }
    }
}
