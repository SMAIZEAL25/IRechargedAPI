using IRecharge_API.Entities;
using IRechargedAPI.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;

namespace IRecharge_API.DAL
{
    public class IRechargeDbContext : DbContext 
    {
        public IRechargeDbContext(DbContextOptions<IRechargeDbContext> options) : base(options)
        {
            
        }
        public DbSet<User> Users { get; set; }
        
        public DbSet<Wallet> wallets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>()
                .Property(u => u.WalletBalance)
                .HasPrecision(18, 2); // Adjust as needed


            modelBuilder.Entity<Wallet>()
                .Property(u => u.Balance)
                .HasPrecision(10, 1);

            modelBuilder.Entity<User>()
                 .HasOne(u => u.Wallet)
                 .WithOne(U => U.User)
                 .HasForeignKey<Wallet>(w => w.UserId);


        }
    }
}
