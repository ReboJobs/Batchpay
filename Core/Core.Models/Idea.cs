using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    [Table("Idea")]
    public partial class Idea
    {
        public Idea()
        {
            IdeaVoteDetails = new HashSet<IdeaVoteDetail>();
        }

        [Key]
        public int Id { get; set; }
        [StringLength(2000)]
        public string? Title { get; set; }
        public string? Description { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? DateCreated { get; set; }
        [StringLength(200)]
        public string? SubmittedBy { get; set; }
        public bool? IsActive { get; set; }

        public string ImageBlobURL { get; set; }

        [InverseProperty(nameof(IdeaVoteDetail.Idea))]
        public virtual ICollection<IdeaVoteDetail> IdeaVoteDetails { get; set; }
    }
}
