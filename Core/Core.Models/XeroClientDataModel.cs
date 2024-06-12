

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    public class XeroClientDataModel
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string Name { get; set; }
        public string XeroClientUniqueId { get; set; }
        public bool IsActive { get; set; }
        public ICollection<XeroUserTenantsSubscription> XeroUserTenantsSubscription { get; set; }
    }
}
