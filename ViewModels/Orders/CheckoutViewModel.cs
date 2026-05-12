using System.ComponentModel.DataAnnotations;
using MobileStore.ViewModels.Cart;

namespace MobileStore.ViewModels.Orders
{
    public class CheckoutViewModel
    {
        [Required, MaxLength(300)]
        public string ShippingAddress { get; set; } = "";

        [Required, MaxLength(100)]
        public string City { get; set; } = "";

        [Required, MaxLength(20), Phone]
        public string Phone { get; set; } = "";

        public string? Notes { get; set; }

        public CartViewModel Cart { get; set; } = new();
    }
}