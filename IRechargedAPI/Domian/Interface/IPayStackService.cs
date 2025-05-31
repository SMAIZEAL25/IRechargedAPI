namespace IRechargedAPI.Domian.Interface
{
    public interface IPayStackService
    {
        Task <(bool Success, string Message)> InitializePaymentAsync(decimal amount, string email, string reference);
    }
}
