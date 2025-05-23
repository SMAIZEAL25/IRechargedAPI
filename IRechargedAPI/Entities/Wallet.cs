using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using IRecharge_API.Entities;

namespace IRechargedAPI.Entities
{
    public class Wallet
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public Guid UserId { get; set; } = Guid.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Balance { get; set; } = 0.00m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual User User { get; set; } = null!;
    }
}