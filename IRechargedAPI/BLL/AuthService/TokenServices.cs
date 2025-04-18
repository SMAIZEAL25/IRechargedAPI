using IRecharge_API.Entities;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace IRecharge_API.BLL.AuthService
{
    public class TokenServices
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<TokenServices> _logger;
        private readonly IConfiguration _configuration;
        private static string _cachedToken;
        private static DateTime _tokenExpiry = DateTime.MinValue;

        public TokenServices(IHttpClientFactory httpClientFactory, ILogger<TokenServices> logger, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<string> GetValidTokenAsync()
        {
            if (!string.IsNullOrEmpty(_cachedToken) && DateTime.UtcNow < _tokenExpiry)
            {
                return _cachedToken;
            }

            try
            {
                // Use named client configured in Program.cs
                var client = _httpClientFactory.CreateClient("DigitalVendorApi");

                // Get configuration values
                var authEndpoint = "auth/login"; // Hardcoded to ensure correctness
                var username = _configuration["AuthSettings:Username"];
                var password = _configuration["AuthSettings:Password"];

                // Add ModSecurity bypass headers
                client.DefaultRequestHeaders.Add("X-Forwarded-For", "127.0.0.1");
                client.DefaultRequestHeaders.Add("X-Request-ID", Guid.NewGuid().ToString());

                var loginRequest = new
                {
                    username,
                    password
                };

                // Serialize manually to ensure proper formatting
                var jsonContent = new StringContent(
                    JsonConvert.SerializeObject(loginRequest),
                    Encoding.UTF8,
                    "application/json");

                var response = await client.PostAsync(authEndpoint, jsonContent);

                if (!response.IsSuccessStatusCode)
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    _logger.LogError($"Auth failed. Status: {response.StatusCode}, Response: {errorContent}");
                    return null;
                }

                var rawResponse = await response.Content.ReadAsStringAsync();
                return ProcessTokenResponse(rawResponse);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Critical error while obtaining auth token");
                return null;
            }
        }

        private string ProcessTokenResponse(string rawResponse)
        {
            try
            {
                // Use custom settings to handle type conversions
                var settings = new JsonSerializerSettings
                {
                    Error = (sender, args) =>
                    {
                        if (args.ErrorContext.Path.Contains("user.id"))
                        {
                            args.ErrorContext.Handled = true;
                        }
                    }
                };

                var tokenResponse = JsonConvert.DeserializeObject<TokenResponses>(rawResponse, settings);

                if (tokenResponse?.token == null)
                {
                    _logger.LogError("Received null token from API");
                    return null;
                }

                _cachedToken = tokenResponse.token;
                _tokenExpiry = DateTime.UtcNow.AddMinutes(tokenResponse.token_validity);
                return _cachedToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to process token response: {rawResponse}");
                return null;
            }
        }

    }
}
