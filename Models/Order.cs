using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileStore.Models
{
    public enum OrderStatus
    {
        Pending,
        Confirmed,
        Shipped,
        Delivered,
        Cancelled
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = "";
        public ApplicationUser User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [Column(TypeName = "decimal(10,2)")]
        public decimal TotalAmount { get; set; }

        // Shipping info
        [MaxLength(300)]
        public string? ShippingAddress { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? Notes { get; set; }

        // Navigation
        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}