using Core;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Persistence.Config;
using System.Text.Json.Serialization;
using XeroApp.Extensions;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xero.NetStandard.OAuth2.Api;
using Xero.NetStandard.OAuth2.Client;
using Xero.NetStandard.OAuth2.Config;
using Xero.NetStandard.OAuth2.Token;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using XeroApp.Services.ApiClientService;
using XeroApp.Services.UserTrackService;
using XeroApp.Services.ReportBugService;
using XeroApp.Services.IdeaService;
using XeroApp.Services.OrganisationService;
using XeroApp.Services.XeroTenantService;
using XeroApp.Services.XeroClientService;
using XeroApp.Services.SubscriptionService;
using XeroApp.Services.ApiLogsService;
using Microsoft.AspNetCore.Http;


namespace XeroApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            services.AddHttpClient("IP", (options) => {
                options.BaseAddress = new Uri("https://jsonip.com");
            });
            services.Configure<XeroConfiguration>(Configuration.GetSection("XeroConfiguration"));

            services.AddDistributedMemoryCache();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(5);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;


            });
            services.AddMvc(options => options.EnableEndpointRouting = false);

            var builder = new DbContextOptionsBuilder<XeroAppDbContext>();
            var connectionString = Configuration.GetSection("Database:ConnectionString").Value;
            builder.UseSqlServer(connectionString);

            services.TryAddSingleton<IXeroClient, XeroClient>();
            services.TryAddSingleton<IAccountingApi, AccountingApi>();
            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.TryAddSingleton(new XeroAppDbContext(builder.Options));
            services.TryAddSingleton<IApiClientService, ApiClientService>();
            services.TryAddSingleton<IUserTrackService, UserTrackService>();
            services.TryAddSingleton<IReportBugService, ReportBugService>();
            services.TryAddSingleton<IIdeaService, IdeaService>();
            services.TryAddSingleton<IOrganisationService, OrganisationService>();
            services.TryAddSingleton<IXeroClientService, XeroClientService>();
            services.TryAddSingleton<IXeroTenantService, XeroTenantService>();
            services.TryAddSingleton<ISubscriptionService, SubscriptionService>();
            services.TryAddSingleton<IApiLogsService, ApiLogsService>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });


            DIRegistration.RegisterDependencies(services);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddMvc().AddRazorRuntimeCompilation();
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(Configuration["SyncfusionLicense"]);
        }



        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/ErrorManager/Index");
            }
            //app.UseExceptionHandler("/ErrorManager/Index");
            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();
            app.UseSession();
            app.UseRouting();
            app.UseMvc();
          
            app.UseAuthentication();
            app.UseAuthorization();

            var culture = new CultureInfo("en-AU");
            culture.DateTimeFormat.ShortDatePattern = "dd/MM/yyyy";
            var supportedCultures = new List<CultureInfo> { culture };
            app.UseRequestLocalization(new RequestLocalizationOptions
            {
                DefaultRequestCulture = new RequestCulture(culture, culture),
                // Formatting numbers, dates, etc.
                SupportedCultures = supportedCultures,
                // UI strings that we have localized.
                SupportedUICultures = supportedCultures
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
