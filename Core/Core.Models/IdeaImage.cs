using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    [Table("IdeaImage")]
    public partial class IdeaImage
    {
        [Key]
        public int Id { get; set; }
        public int? IdeaId { get; set; }
        [StringLength(1000)]
        public string? FileName { get; set; }
        [Column("ImageBlobURL")]
        public string? ImageBlobUrl { get; set; }
        [StringLength(255)]
        public string? UserName { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? DateCreated { get; set; }
    }
}
