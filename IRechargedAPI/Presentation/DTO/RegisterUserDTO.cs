namespace IRechargedAPI.Presentation.DTO
{
    public class RegisterUserDTO
    {
        public required string UserName { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public decimal WalletBalance { get; set; }

        public required string PhoneNumber { get; set; }

        public required string role { get; set; }
    }
}
