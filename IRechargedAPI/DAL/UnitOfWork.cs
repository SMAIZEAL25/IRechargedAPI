using IRecharge_API.DAL;

namespace IRechargedAPI.DAL
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

        public async Task<int> SaveChangeAsync()
        {
            return await _rechargeDbContext.SaveChangesAsync();
        }

        public void Dispose ()
        {
            _rechargeDbContext.Dispose();
        }

       
    }
}
