using IRecharge_API.BLL.AuthService;
using IRecharge_API.DTO;
using IRechargedAPI.BLL.AuthService;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IRechargedAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAuthManager _AuthManager;
        public AccountController(IAuthManager authManager)
        {
            _AuthManager = authManager;
        }

        [HttpPost("Register")]
        public async Task<APIResponse<AuthReponse>> Register(RegisterUserDTO registerUserDTO) 
        {
         
            return await _AuthManager.Register(registerUserDTO);
            
        }


        [HttpPost("Login")]
        public async Task<APIResponse<AuthReponse>> Login(LoginDto loginDto) 
        {
            return await _AuthManager.Login(loginDto); 

        }
    }
}
