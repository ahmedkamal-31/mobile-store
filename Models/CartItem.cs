using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileStore.Models
{
    // ── Cart Item ──────────────────────────────────────────────────────────
    public class CartItem
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; } = null!;

        public int PhoneId { get; set; }
        public Phone Phone { get; set; } = null!;

        public int Quantity { get; set; } = 1;
        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }

}