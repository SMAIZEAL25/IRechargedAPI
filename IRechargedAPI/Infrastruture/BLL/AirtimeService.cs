using Newtonsoft.Json;
using System.Text;
using System.Net.Http.Headers;
using IRechargedAPI.Infrastruture.ExternalServices.Models;
using IRechargedAPI.Infrastruture.BLL.AuthService;

namespace IRechargedAPI.Infrastruture.BLL
{
    public class AirtimeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly TokenServices _tokenService;
        private readonly ILogger<AirtimeService> _logger;
        private readonly IConfiguration _configuration;

        public AirtimeService(
            IHttpClientFactory httpClientFactory,
            TokenServices tokenService,
            ILogger<AirtimeService> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _tokenService = tokenService;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<DigitalVendorsReponseModel> PurchaseAirtime(VendAirtimeRequestModel requestModel)
        {
            var responseModel = new DigitalVendorsReponseModel();

            // Get base URL from configuration
            var baseUrl = _configuration["DigitalVendorsUrl:BaseUrl"];

            if (string.IsNullOrEmpty(baseUrl))
            {
                responseModel.isSuccessful = false;
                responseModel.responsemessage = "API base URL configuration missing";
                _logger.LogError("DigitalVendorsUrl:BaseUrl is missing in configuration");
                return responseModel;
            }

            try
            {
                // Get valid token
                var authToken = await _tokenService.GetValidTokenAsync();
                if (string.IsNullOrEmpty(authToken))
                {
                    responseModel.isSuccessful = false;
                    responseModel.responsemessage = "Failed to obtain authentication token";
                    return responseModel;
                }

                // Use named client
                var client = _httpClientFactory.CreateClient("DigitalVendorApi");

                // Construct full endpoint URL
                var requestUrl = $"{baseUrl.TrimEnd('/')}/airtime";

                // Create and configure request
                var request = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = new StringContent(
                        JsonConvert.SerializeObject(requestModel),
                        Encoding.UTF8,
                        "application/json")
                };

                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", authToken);
                request.Headers.Add("X-Requested-With", "XMLHttpRequest");

                // Execute request
                var response = await client.SendAsync(request);
                var responseString = await response.Content.ReadAsStringAsync();

                _logger.LogDebug($"API Response: {response.StatusCode} - {responseString}");

                if (!response.IsSuccessStatusCode)
                {
                    responseModel.isSuccessful = false;
                    responseModel.responsemessage = $"API request failed ({(int)response.StatusCode})";
                    return responseModel;
                }

                return JsonConvert.DeserializeObject<DigitalVendorsReponseModel>(responseString) ?? responseModel;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Airtime purchase failed");
                responseModel.isSuccessful = false;
                responseModel.responsemessage = ex.Message;
                return responseModel;
            }
        }
    }
}
