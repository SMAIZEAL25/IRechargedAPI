using IRecharge_API.DTO;
using IRecharge_API.Entities;

namespace IRecharge_API.DAL
{
    public interface IUserRepository
    {
        User GetUserByPhoneNumber(string phoneNumber);

        void UpdateUserAsync(User user);

        User GetUserById(Guid id);

        User GetByUserName(string userName);

        void SaveChange(User registerUserDTO);

        Task AddUserAsync(User user);
    }
}
