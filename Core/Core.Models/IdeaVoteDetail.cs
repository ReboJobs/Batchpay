using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Core
{
    [Table("IdeaVoteDetail")]
    public partial class IdeaVoteDetail
    {
        [Key]
        public int Id { get; set; }
        public int? IdeaId { get; set; }
        [StringLength(200)]
        public string? VotedBy { get; set; }
        [Column(TypeName = "datetime")]
        public DateTime? DateVoted { get; set; }
        public bool? IsActive { get; set; }

        [ForeignKey(nameof(IdeaId))]
        [InverseProperty("IdeaVoteDetails")]
        public virtual Idea? Idea { get; set; }
    }
}
