using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence
{
    internal class ReportBugConfig : IEntityTypeConfiguration<ReportBug>
    {
        public void Configure(EntityTypeBuilder<ReportBug> builder)
        {
            builder.HasKey(x => x.Id);
        }
    }
}

