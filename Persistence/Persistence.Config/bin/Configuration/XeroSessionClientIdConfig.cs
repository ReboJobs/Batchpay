using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Config
{
	internal class XeroSessionClientIdConfig : IEntityTypeConfiguration<XeroSessionClientId>
    {
        public void Configure(EntityTypeBuilder<XeroSessionClientId> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(p => p.UserName).IsRequired();
            builder.Property(p => p.ClientId).IsRequired();
        }
    }
}