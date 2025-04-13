using AutoMapper;
using IRecharge_API.DTO;
using Microsoft.AspNetCore.Identity;

namespace IRecharge_API.BLL.AuthService
{
    public class AuthManager : IAuthManager
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly IMapper _mapper;

        public AuthManager(UserManager<IdentityUser> userManager, 
            RoleManager<IdentityRole> roleManager,
            IMapper mapper
            
        )
        {
           _userManager = userManager;
           _roleManager = roleManager;
           _mapper = mapper;
        }




        public Task<APIResponse<object>> Login(LoginDto loginDTO)
        {
            throw new NotImplementedException();
        }

        public Task<APIResponse<object>> Register(RegisterUserDTO createStudentDTO)
        {
            throw new NotImplementedException();
        }
    }
}
