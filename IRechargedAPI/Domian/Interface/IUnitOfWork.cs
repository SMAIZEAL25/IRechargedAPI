using IRechargedAPI.Domian.Entities;

namespace IRechargedAPI.Domian.Interface
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository UserRepository { get; }
        Task<int> SaveChangeAsync(User userRecord);
    }
}
