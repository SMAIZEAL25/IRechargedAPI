using IRecharge_API.BLL.AuthService;
using IRecharge_API.DTO;
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
        public async Task<IActionResult> Register(RegisterUserDTO registerUserDTO) 
        {
            var result = await _AuthManager.Register(registerUserDTO);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginDto loginDto) 
        {
            var result = await _AuthManager.Login(loginDto);
            if (result.IsSuccess)
                return Ok(result);

            return BadRequest(result);

        }
    }
}
