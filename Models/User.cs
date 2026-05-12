using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MobileStore.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = "";

        public string? ProfileImageUrl { get; set; }
        public bool IsSeller { get; set; } = false;
        public bool IsBlocked { get; set; } = false;
        public string? ShopAddress { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}