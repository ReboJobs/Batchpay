using Accounts.Config;
using XeroApp.Models.BusinessModels.Mappings;
using XeroApp.Services;

namespace XeroApp.Extensions
{
	public static class DIRegistration
	{
		public static IServiceCollection RegisterDependencies(this IServiceCollection services)
		{
			services.ConfigureMappingProfile();

			services.RegisterIdentity();

			services.AddScoped<IExportService, ExportService>();
			services.AddScoped<IXeroService, XeroService>();
			services.AddScoped<IUserService, UserService>();
			services.AddScoped<IEmailService, EmailService>();

			return services;
		}
	}
}