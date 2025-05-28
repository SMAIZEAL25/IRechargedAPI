namespace IRechargedAPI.Domian.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public required string UserName { get; set; }

        public required string Email { get; set; }

        public decimal WalletBalance { get; set; }

        public required string PhoneNumber { get; set; }

        public DateTime DateCreated { get; set; }

        public string IdentityUserId { get; set; }

        public Wallet Wallet { get; set; }
    }
}
