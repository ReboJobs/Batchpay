using XeroApp.Models.BusinessModels;

namespace XeroApp.Services.ApiClientService
{

    public class ApiClientService : IApiClientService
    {
        private readonly IHttpClientFactory _httpClientFactory;


        public ApiClientService(IHttpClientFactory httpClientfactory)
        {
            _httpClientFactory = httpClientfactory;
        }

        public async Task<UserTrackModel> GetUserIPAsync()
        {
            var client = _httpClientFactory.CreateClient("IP");
            return await client.GetFromJsonAsync<UserTrackModel>("/");
        }
    }

    
}
