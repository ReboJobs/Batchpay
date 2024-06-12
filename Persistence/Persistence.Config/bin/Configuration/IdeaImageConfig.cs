

using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence
{
    internal class IdeaImageConfig : IEntityTypeConfiguration<IdeaImage>
    {
        public void Configure(EntityTypeBuilder<IdeaImage> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}



