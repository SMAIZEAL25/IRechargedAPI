using IRechargedAPI.Domian.Entities;
using IRechargedAPI.Domian.Interface;

namespace IRechargedAPI.Infrastruture.BLL
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly IRechargeDbContext _rechargeDbContext;

        public IUserRepository UserRepository { get; private set; }


        public UnitOfWork(IRechargeDbContext rechargeDbContext)
        {
            _rechargeDbContext = rechargeDbContext;
            UserRepository = new UserRepository(_rechargeDbContext);
        }

        public async Task<int> SaveChangeAsync(User userRecord)
        {
            return await _rechargeDbContext.SaveChangesAsync();
        }

        public void Dispose()
        {
            _rechargeDbContext.Dispose();
        }


    }
}
