using MobileStore.Models;

namespace MobileStore.ViewModels
{
    public class OrderItemLine
    {
        public string PhoneName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => UnitPrice * Quantity;
    }

    public class SellerOrderViewModel
    {
        public int OrderId { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public decimal TotalAmount { get; set; }

        // Customer info
        public string CustomerName { get; set; } = "";
        public string CustomerEmail { get; set; } = "";
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }

        // Items (seller's items in this order)
        public List<OrderItemLine> Items { get; set; } = new();
    }
}
