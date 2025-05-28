namespace IRechargedAPI.Domian.Interface
{
    public interface IPaymentService
    {
        Task<decimal> GetBalanceAsync(Guid userId);
        Task<decimal> TopUpwalletAsync(Guid UserId, decimal amount, string paymentReference);
    }
}
