using AutoMapper;
using Core;
using Xero.NetStandard.OAuth2.Model.Accounting;
using Xero.NetStandard.OAuth2.Token;

namespace XeroApp.Models.BusinessModels.Mappings
{
	public class AutoMapperRegistration : Profile
	{
        public AutoMapperRegistration()
        {
            CreateMap<XeroClientApp, XeroClientAppModel>();
            CreateMap<XeroClientAppModel, XeroClientApp>();
            CreateMap<XeroSessionClientId, XeroSessionClientIdModel>();
            CreateMap<XeroOAuth2Token, XeroToken>();
            CreateMap<XeroTokenModel, XeroToken>();
            CreateMap<InherittedInvoice2, Invoice>()
                .ForMember(dest => dest.InvoiceID, from => from.MapFrom(src => src.InvoiceID))
                .ForMember(dest => dest.Status, from => from.MapFrom(src => src.Status));
        }
    }

    public static class MapperRegistration
    {
        public static IServiceCollection ConfigureMappingProfile(this IServiceCollection services)
        {
            services.AddAutoMapper(typeof(MapperRegistration));

            return services;
        }
    }
}