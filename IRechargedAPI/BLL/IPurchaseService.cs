using IRecharge_API.DTO;
using IRechargedAPI.Entities;

namespace IRecharge_API.BLL
{
    public interface IPurchaseService
    {
        Task<decimal> GetBalanceAsync(Guid userId);
        Task<ResponseModel> PurchaseAirtimeService(PurchaseAirtimeRequestDTO purchaseAirtimeRequestDTO, string username);
        Task<Wallet> TopUpWalletAsync(Guid userId, decimal amount);
    }
}
