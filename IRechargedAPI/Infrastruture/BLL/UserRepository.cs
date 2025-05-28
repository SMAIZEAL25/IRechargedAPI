using AutoMapper;
using IRechargedAPI.Domian.Entities;
using IRechargedAPI.Domian.Interface;
using Microsoft.EntityFrameworkCore;

namespace IRechargedAPI.Infrastruture.BLL
{
    public class UserRepository : IUserRepository
    {
        // The IRechargeDbContext is used to interact with the database.
        private readonly IRechargeDbContext _context;


        // Constructor that initializes the UserRepository with a database context and an AutoMapper instance.
        public UserRepository(IRechargeDbContext context)
        {
            _context = context;

        }

        // This method retrieves a user by their username from the database.
        public User? GetByUserName(string userName)
        {
            return _context.Users.FirstOrDefault(u => u.UserName == userName);
        }

        // This method retrieves a user by their ID from the database.
        public User? GetUserById(Guid id)
        {
            return _context.Users.SingleOrDefault(u => u.Id == id);
        }

        // This method retrieves a user by their phone number from the database.
        public User? GetUserByPhoneNumber(string phoneNumber)
        {
            return _context.Users.FirstOrDefault(u => u.PhoneNumber == phoneNumber);
        }

        // This method is used to save a new user to the database.
        public void SaveChange(User registerUserDTO)
        {
            _context.Users.Add(registerUserDTO);
            _context.SaveChanges();

        }
        // This method is not used in the current implementation, but it can be used to update user information.

        public void UpdateUserAsync(User user)
        {
            var updateUserRecord = _context.Users.Update(user);
        }

        public async Task AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
        }

        public async Task<Wallet?> GetWalletAsync(Guid userId)
        {
            return await _context.wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        }

        public async Task<Wallet> CreateWalletAsync(Guid Userid)
        {
            var wallet = new Wallet
            {
                UserId = Userid,
                Balance = 0.00m
            };

            _context.wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return wallet;
        }

        public void UpdateWalletAsync(Wallet wallet)
        {
            var response = _context.wallets.Update(wallet);
            _context.SaveChanges();
        }
    }
}