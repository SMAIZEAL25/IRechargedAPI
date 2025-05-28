using System.Globalization;

namespace IRechargedAPI.Infrastruture.BLL.AuthService
{
    public class AuthReponse
    {
        public required string UserName { get; set; }

        public required string Email { get; set; }

        public decimal WalletBalance { get; set; }
        public required string PhoneNumber { get; set; }
        //public required string EmailConfirmationToken { get; set; }
    }
}
