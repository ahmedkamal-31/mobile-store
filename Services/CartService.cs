using Microsoft.EntityFrameworkCore;
using MobileStore.Data;
using MobileStore.Models;
using MobileStore.ViewModels.Cart;

namespace MobileStore.Services
{
    public class CartService
    {
        private readonly AppDbContext _db;

        public CartService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<CartViewModel> GetCartAsync(string userId)
        {
            var items = await _db.CartItems
                .Include(c => c.Phone)
                .ThenInclude(p => p.Brand)
                .Include(c => c.Phone)
                .ThenInclude(p => p.Seller)
                .Where(c => c.UserId == userId)
                .ToListAsync();

            return new CartViewModel
            {
                Items = items
            };
        }

        public async Task<bool> AddToCartAsync(string userId, int phoneId, int quantity = 1)
        {
            var phone = await _db.Phones.FindAsync(phoneId);

            if (phone == null || !phone.IsAvailable || phone.Stock < quantity)
                return false;

            var existing = await _db.CartItems
                .FirstOrDefaultAsync(c => c.UserId == userId && c.PhoneId == phoneId);

            if (existing != null)
            {
                existing.Quantity += quantity;
            }
            else
            {
                _db.CartItems.Add(new CartItem
                {
                    UserId = userId,
                    PhoneId = phoneId,
                    Quantity = quantity
                });
            }

            await _db.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UpdateCartItemQuantityAsync(string userId, int cartItemId, int quantity)
        {
            if (quantity < 1) return false;

            var item = await _db.CartItems
                .Include(c => c.Phone)
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (item == null || item.Phone == null) return false;

            if (quantity > item.Phone.Stock) return false;

            item.Quantity = quantity;
            await _db.SaveChangesAsync();
            return true;
        }

        public async Task RemoveFromCartAsync(string userId, int cartItemId)
        {
            var item = await _db.CartItems
                .FirstOrDefaultAsync(c => c.Id == cartItemId && c.UserId == userId);

            if (item != null)
            {
                _db.CartItems.Remove(item);
                await _db.SaveChangesAsync();
            }
        }

        public async Task<int> GetCartCountAsync(string userId)
        {
            return await _db.CartItems
                .Where(c => c.UserId == userId)
                .SumAsync(c => c.Quantity);
        }

        public async Task ClearCartAsync(string userId)
        {
            var items = _db.CartItems.Where(c => c.UserId == userId);
            _db.CartItems.RemoveRange(items);
            await _db.SaveChangesAsync();
        }
    }
}