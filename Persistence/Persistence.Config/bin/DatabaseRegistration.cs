using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Config
{
    public static class DatabaseRegistration
    {
        public static void RegisterDatabase(this IServiceCollection services, IConfiguration config)
        {
            services.AddDbContext<XeroAppDbContext>(options =>
                options.UseSqlServer(
                    config.GetSection("Database:ConnectionString").Value,
                    x => x.MigrationsAssembly("Persistence.Config")
                ));
        }
    }
}
