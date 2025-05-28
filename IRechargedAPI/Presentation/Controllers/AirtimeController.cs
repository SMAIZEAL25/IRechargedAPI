
using IRechargedAPI.Domian.Interface;
using IRechargedAPI.Infrastruture.BLL;
using IRechargedAPI.Infrastruture.BLL.AuthService;
using IRechargedAPI.Infrastruture.ExternalServices.Models;
using Microsoft.AspNetCore.Mvc;

namespace IRechargedAPI.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AirtimeController : ControllerBase
    {
        private readonly AirtimeService _airtimeService;
        private readonly TokenServices _tokenService;
        private readonly IPurchaseService _purchaseService;

        public AirtimeController(AirtimeService airtimeService, TokenServices tokenService, IPurchaseService purchaseService)
        {
            _airtimeService = airtimeService;
            _tokenService = tokenService;
            _purchaseService = purchaseService;
        }

        [HttpGet("token")]
        public async Task<IActionResult> GetApiTokenAsync()
        {
            var token = await _tokenService.GetValidTokenAsync();
            return string.IsNullOrEmpty(token)
                ? StatusCode(500, "Failed to get token")
                : Ok(new { Token = token });
        }

        [HttpPost("purchase")]
        public async Task<IActionResult> PurchaseAirtime([FromBody] VendAirtimeRequestModel request)
        {
            var result = await _airtimeService.PurchaseAirtime(request);
            return result.isSuccessful
                ? Ok(result)
                : BadRequest(result);
        }

        [HttpGet("getuserbalance")]
        public async Task<IActionResult> GetUserBalance(Guid Userid)
        {
            var user = await _purchaseService.GetBalanceAsync(Userid);
            if (user == null)
            {
                return NotFound("User not found");
            }
            return Ok(user);
        }


        [HttpPost("TopUpUserWallet")]
        public async Task<IActionResult> TopUpUserWallet([FromBody] Guid UserId, decimal amount)
        {
            var result = await _purchaseService.TopUpWalletAsync(UserId, amount);

            if (result != null)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest("Failed to top up wallet.");
            }
        }
    }
}
