using XeroApp.Services;

namespace XeroApp.Extensions
{
	public static class ServiceExtensions
	{
		public static IServiceCollection RegisterServices(this IServiceCollection services)
		{
			services.AddScoped<IExportService, ExportService>();
			services.AddScoped<IXeroService, XeroService>();

			return services;
		}
	}
}