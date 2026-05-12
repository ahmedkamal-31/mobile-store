using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MobileStore.Models
{
    // ── Brand ──────────────────────────────────────────────────────────────
    public class Brand
    {
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Name { get; set; } = "";

        public string? LogoUrl { get; set; }

        public ICollection<Phone> Phones { get; set; } = new List<Phone>();
    }

    // ── Phone Condition ────────────────────────────────────────────────────
    public enum PhoneCondition
    {
        New,
        Used,
        LikeNew
    }

    // ── Phone ──────────────────────────────────────────────────────────────
    public class Phone
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = "";

        public string? Slug { get; set; }           // SEO-friendly URL slug

        [Column(TypeName = "decimal(10,2)")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(10,2)")]
        public decimal? OldPrice { get; set; }         // For discount display

        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;

        // ── Specs ──────────────────────────────────────────────────────────
        public string? ImageUrl { get; set; }
        public string? ImageGallery { get; set; }      // JSON array of image URLs

        [MaxLength(50)]
        public string? Color { get; set; }

        public int RAM { get; set; }        // GB
        public int Storage { get; set; }        // GB
        public double ScreenSize { get; set; }        // inches
        public int Battery { get; set; }        // mAh

        [MaxLength(100)]
        public string? Processor { get; set; }

        [MaxLength(100)]
        public string? Camera { get; set; }        // e.g. "50MP + 12MP + 10MP"

        [MaxLength(50)]
        public string? OS { get; set; }        // e.g. "Android 14"

        [MaxLength(50)]
        public string? Network { get; set; }        // e.g. "5G"

        [MaxLength(2000)]
        public string? Description { get; set; }

        public PhoneCondition Condition { get; set; } = PhoneCondition.New;

        public int Stock { get; set; } = 0;
        public bool IsAvailable { get; set; } = true;
        public bool IsFeatured { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ── Seller ownership ────────────────────────────────────────────────
        public string? SellerId { get; set; }
        public ApplicationUser? Seller { get; set; }

        // ── Navigation ─────────────────────────────────────────────────────
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}