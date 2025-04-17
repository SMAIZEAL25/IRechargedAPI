using IRecharge_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace IRecharge_API.DAL
{
    public class IRechargeDbContext : DbContext 
    {
        public IRechargeDbContext(DbContextOptions<IRechargeDbContext> options) : base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.WalletBalance)
                .HasPrecision(18, 2); // Adjust as needed
        }
    }
}
