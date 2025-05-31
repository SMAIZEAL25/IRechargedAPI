using IRechargedAPI.Domian.Interface;

namespace IRechargedAPI.Infrastruture.BLL.AuthService
{
    public class PayStackServices : IPayStackService
    {
        private readonly HttpClient _httpClient;
        private readonly string _paystackBaseUrl = "https://api.paystack.co";

        public PayStackServices(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }
        public Task<(bool Success, string Message)> InitializePaymentAsync(decimal amount, string email, string reference)
        {
            throw new NotImplementedException();
        }
    }
}
