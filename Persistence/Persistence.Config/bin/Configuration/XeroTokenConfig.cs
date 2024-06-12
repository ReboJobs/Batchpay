using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Config
{
	internal class XeroTokenConfig : IEntityTypeConfiguration<XeroToken>
    {
        public void Configure(EntityTypeBuilder<XeroToken> builder)
        {
            builder.HasKey(p => p.Id);
            builder.Property(p => p.UserName).IsRequired();
            builder.Property(p => p.AccessToken).IsRequired();
            builder.Property(p => p.RefreshToken).IsRequired();
            builder.Property(p => p.IdToken).IsRequired();
            builder.Property(p => p.ExpiresAtUtc).IsRequired();
        }
    }
}