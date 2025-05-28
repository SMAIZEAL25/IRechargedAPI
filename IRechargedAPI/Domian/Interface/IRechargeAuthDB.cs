using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace IRechargedAPI.Domian.Interface
{
    public class IRechargeAuthDB : IdentityDbContext
    {
        public IRechargeAuthDB(DbContextOptions<IRechargeAuthDB> options) : base(options)

        {

        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            var readerId = "5bc84df1-b43d-4830-b8b8-ccf750839ae2";
            var writerId = "d76b6975-c526-4dc1-af45-22c4bb248f54";



            var roles = new List<IdentityRole>
            {
                new IdentityRole
                {
                    Id = readerId,
                    ConcurrencyStamp = readerId, //ConcurrencyStamp = Guid.NewGuid().ToString(),
                    Name = "Reader",
                    NormalizedName = "Reader".ToUpper(),
                },


                new IdentityRole
                {
                    Id = writerId,
                    ConcurrencyStamp = writerId,
                    Name = "Writer",
                    NormalizedName = "Writer".ToUpper(),
                }

            };

            builder.Entity<IdentityRole>()
                .HasData(roles);
        }

    }
}// Compare this snippet from IRecharge_API/Program.cs:
