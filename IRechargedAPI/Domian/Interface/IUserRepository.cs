using IRechargedAPI.Domian.Entities;

namespace IRechargedAPI.Domian.Interface
{
    public interface IUserRepository
    {
        User GetUserByPhoneNumber(string phoneNumber);

        void UpdateUserAsync(User user);

        User GetUserById(Guid id);

        User GetByUserName(string userName);

        void SaveChange(User registerUserDTO);

        Task AddUserAsync(User user);

        Task<Wallet?> GetWalletAsync(Guid userId);

        Task<Wallet> CreateWalletAsync(Guid Userid);

        void UpdateWalletAsync(Wallet amount);
    }
}
