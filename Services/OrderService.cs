using MobileStore.Data;
using MobileStore.Models;
using MobileStore.ViewModels.Orders;

namespace MobileStore.Services
{
    public class OrderService
    {
        private readonly AppDbContext _db;
        private readonly CartService _cart;

        public OrderService(AppDbContext db, CartService cart)
        {
            _db = db;
            _cart = cart;
        }

        public async Task<Order?> PlaceOrderAsync(string userId, CheckoutViewModel checkout)
        {
            var cart = await _cart.GetCartAsync(userId);

            if (!cart.Items.Any())
                return null;

            // Validate stock before placing order
            foreach (var item in cart.Items)
            {
                var phone = await _db.Phones.FindAsync(item.PhoneId);
                if (phone == null || phone.Stock < item.Quantity || !phone.IsAvailable)
                    return null;
            }

            var phones = new List<Phone>();
            foreach (var item in cart.Items)
            {
                var phone = await _db.Phones.FindAsync(item.PhoneId);
                if (phone != null)
                {
                    phone.Stock -= item.Quantity;
                    phones.Add(phone);
                }
            }

            var order = new Order
            {
                UserId = userId,
                ShippingAddress = checkout.ShippingAddress,
                City = checkout.City,
                Phone = checkout.Phone,
                Notes = checkout.Notes,
                TotalAmount = cart.Total,
                Status = OrderStatus.Pending,
            };

            foreach (var item in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    PhoneId = item.PhoneId,
                    Quantity = item.Quantity,
                    UnitPrice = item.Phone.Price
                });
            }

            _db.Orders.Add(order);
            await _db.SaveChangesAsync();

            await _cart.ClearCartAsync(userId);

            return order;
        }
    }
}