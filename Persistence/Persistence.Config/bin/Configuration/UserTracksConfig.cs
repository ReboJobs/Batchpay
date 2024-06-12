using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;


namespace Persistence.Config
{
    internal class UserTracksConfig : IEntityTypeConfiguration<UserTrack>
    {
        public void Configure(EntityTypeBuilder<UserTrack> builder)
        {
            builder.HasKey(x => x.TrackId);
        }
    }
}
