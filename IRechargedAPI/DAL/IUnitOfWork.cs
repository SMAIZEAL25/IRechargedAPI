using IRecharge_API.DAL;

namespace IRechargedAPI.DAL
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        Task<int> SaveChangeAsync();
    }
}
