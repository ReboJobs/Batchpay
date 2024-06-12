using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Config
{
	internal class XeroClientConfig : IEntityTypeConfiguration<XeroClientApp>
    {
        public void Configure(EntityTypeBuilder<XeroClientApp> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(p => p.ClientId).IsRequired();
            builder.Property(p => p.CallbackUri).IsRequired();
            builder.Property(p => p.Scope).IsRequired();
            builder.Property(p => p.State).IsRequired();
            builder.Property(p => p.UserName).IsRequired();
        }
    }
}