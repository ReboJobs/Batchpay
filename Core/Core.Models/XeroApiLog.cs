using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    public partial class XeroApiLog
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }
        [Column("CreateDateUTC", TypeName = "datetime")]
        public DateTime CreateDateUtc { get; set; }
        [StringLength(250)]
        public string TenantName { get; set; }
        [StringLength(50)]
        public string Status { get; set; }
        [StringLength(100)]
        public string Type { get; set; }
        [StringLength(250)]
        public string Url { get; set; }
        [StringLength(50)]
        public string Method { get; set; }
        [Column("TenantID")]
        public Guid TenantId { get; set; }
    }
}
