using MobileStore.Models;

namespace MobileStore.ViewModels.Cart
{
    public class CartViewModel
    {
        public IEnumerable<CartItem> Items { get; set; } = new List<CartItem>();

        public decimal Total => Items.Sum(i => i.Phone.Price * i.Quantity);
        public int ItemCount => Items.Sum(i => i.Quantity);
    }
}