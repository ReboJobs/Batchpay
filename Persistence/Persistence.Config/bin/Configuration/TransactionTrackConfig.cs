using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence
{
    internal class TransactionTrackConfig : IEntityTypeConfiguration<TransactionTrack>
    {
        public void Configure(EntityTypeBuilder<TransactionTrack> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TransactionValues).HasColumnType("nvarchar(max)");
        }

    }
}
