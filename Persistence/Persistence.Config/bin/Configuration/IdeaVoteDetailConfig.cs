

using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence
{
    internal class IdeaVoteDetailConfig : IEntityTypeConfiguration<IdeaVoteDetail>
    {
        public void Configure(EntityTypeBuilder<IdeaVoteDetail> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}


