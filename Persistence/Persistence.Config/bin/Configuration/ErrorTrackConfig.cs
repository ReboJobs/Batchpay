using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence
{
    internal class ErrorTrackConfig : IEntityTypeConfiguration<ErrorTrack>
    {
        public void Configure(EntityTypeBuilder<ErrorTrack> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.TransactionValues).HasColumnType("nvarchar(max)");
            builder.Property(x => x.ErrorMessage).HasColumnType("nvarchar(max)");
            builder.Property(x => x.ErrorMessageDetail).HasColumnType("nvarchar(max)");

        }
    }
}
