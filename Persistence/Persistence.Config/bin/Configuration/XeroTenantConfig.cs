using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence
{
    internal class XeroTenantConfig : IEntityTypeConfiguration<XeroTenant>
    {
        public void Configure(EntityTypeBuilder<XeroTenant> builder)
        {
            builder.HasKey(x => x.Id);
            builder.HasMany(x => x.XeroUserTenantsSubscription)
             .WithOne(x => x.XeroTenant)
             .HasForeignKey(x => x.XeroTenantId);
        }
    }
}
