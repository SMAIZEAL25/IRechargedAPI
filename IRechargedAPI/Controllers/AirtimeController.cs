
using IRecharge_API.BLL;
using IRecharge_API.BLL.AuthService;
using IRecharge_API.ExternalServices.Models;
using IRechargedAPI.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace IRecharge_API.Controllers
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


        [HttpPost  ("TopUpUserWallet")]
        public async Task<IActionResult> TopUpUserWallet([FromBody] Guid UserId)
        {
            var result = await _purchaseService.TopUpWalletAsync(UserId);
            return result.isSuccessful
                ? Ok(result)
                : BadRequest(result);
        }
    }
}
