using System.ComponentModel.DataAnnotations.Schema;

namespace MobileStore.Models
{
    public class OrderItem
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int PhoneId { get; set; }
        public Phone Phone { get; set; } = null!;

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(10,2)")]
        public decimal UnitPrice { get; set; }         // Snapshot price at time of order

        [Column(TypeName = "decimal(10,2)")]
        public decimal Subtotal => UnitPrice * Quantity;
    }
}