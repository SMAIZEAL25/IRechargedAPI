using AutoMapper;
using IRecharge_API.DTO;
using IRecharge_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace IRecharge_API.DAL
{
    public class UserRepository : IUserRepository
    {
        // The IRechargeDbContext is used to interact with the database.
        private readonly IRechargeDbContext _context;
        // The IMapper is used for mapping between DTOs and entities.
        private readonly IMapper mapper;

        // Constructor that initializes the UserRepository with a database context and an AutoMapper instance.
        public UserRepository(IRechargeDbContext context, IMapper mapper)
        {
            _context = context;
            this.mapper = mapper;
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
            var userEntity = mapper.Map<User>(registerUserDTO);
            _context.Users.Add(userEntity);
            _context.SaveChanges();

        }
        // This method is not used in the current implementation, but it can be used to update user information.

        public void UpdateUserAsync(User user)
        {
            var updateUserRecord = _context.Users.Update(user);
        }

    }
}
