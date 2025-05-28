using System.ComponentModel.DataAnnotations;

namespace IRechargedAPI.Presentation.DTO
{
    public class PurchaseAirtimeRequestDTO
    {
        [Required(ErrorMessage = "Phone number is required")]
        public required string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Amount is required")]
        public decimal Amount { get; set; }

        public required string NetworkType { get; set; }
    }
}
