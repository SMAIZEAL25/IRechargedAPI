namespace IRechargedAPI.Presentation.DTO
{
    public class PurchaseDataRequestDTO
    {
        public required string PhoneNumber { get; set; }
        public decimal Amount { get; set; }
        public required string NetworkType { get; set; }
    }
}
