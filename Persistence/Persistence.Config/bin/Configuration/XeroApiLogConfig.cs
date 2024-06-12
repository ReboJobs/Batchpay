
using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence
{
    internal class XeroApiLogConfig : IEntityTypeConfiguration<XeroApiLog>
    {
        public void Configure(EntityTypeBuilder<XeroApiLog> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}


