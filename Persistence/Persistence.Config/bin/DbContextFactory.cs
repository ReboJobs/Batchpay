using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Persistence.Config
{
    public class DbContextFactory : IDesignTimeDbContextFactory<XeroAppDbContext>
    {
        public IConfiguration Configuration { get; }
        public DbContextFactory(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public DbContextFactory()
        {

        }

        public XeroAppDbContext CreateDbContext(string[] args)
        {
            var builder = new DbContextOptionsBuilder<XeroAppDbContext>();
            builder.UseSqlServer("Data Source=expenseapidb.database.windows.net;Initial Catalog=XeroApps;User Id=dbadmin;Password=P@ssw0rd;TrustServerCertificate=true;MultipleActiveResultSets=True");
            return new XeroAppDbContext(builder.Options);
        }
    }
}
