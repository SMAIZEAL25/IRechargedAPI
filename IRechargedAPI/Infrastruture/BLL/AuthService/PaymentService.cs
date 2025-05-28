using IRechargedAPI.Domian.Interface;

namespace IRechargedAPI.Infrastruture.BLL.AuthService
{
    public class PaymentService : IPaymentService
    {
        private readonly IUserRepository _userRepository;

        public PaymentService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        Task<decimal> IPaymentService.GetBalanceAsync(Guid userId)
        {
            return null;
        }

        Task<decimal> IPaymentService.TopUpwalletAsync(Guid UserId, decimal amount, string paymentReference)
        {
            return null;
        }
    }
}
