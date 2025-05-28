using IRechargedAPI.Domian.Entities;
using IRechargedAPI.Presentation.DTO;

namespace IRechargedAPI.Domian.Interface
{
    public interface IPurchaseService
    {
        Task<decimal> GetBalanceAsync(Guid userId);
        Task<ResponseModel> PurchaseAirtimeService(PurchaseAirtimeRequestDTO purchaseAirtimeRequestDTO, string username);
        Task<Wallet> TopUpWalletAsync(Guid userId, decimal amount);
    }
}
