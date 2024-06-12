using Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Persistence.Config
{
    internal class FinancialInstitutionCodeConfig: IEntityTypeConfiguration<FinancialInstitutionCode>
    {

        public void Configure(EntityTypeBuilder<FinancialInstitutionCode> builder)
        {
            builder.HasKey(x => x.IdfinancialInstitutionCodes);
        }
    }
}
