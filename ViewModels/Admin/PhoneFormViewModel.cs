using System.ComponentModel.DataAnnotations;
using MobileStore.Models;

namespace MobileStore.ViewModels.Admin
{
    public class PhoneFormViewModel
    {
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = "";

        [Required]
        public int BrandId { get; set; }

        [Required, Range(0, 9999999)]
        public decimal Price { get; set; }

        public decimal? OldPrice { get; set; }

        [Range(1, 64)]
        public int RAM { get; set; }

        [Range(8, 2048)]
        public int Storage { get; set; }

        [Range(3.0, 8.0)]
        public double ScreenSize { get; set; }

        [Range(1000, 10000)]
        public int Battery { get; set; }

        public string? Processor   { get; set; }
        public string? Camera      { get; set; }
        public string? OS          { get; set; }
        public string? Network     { get; set; }
        public string? Color       { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl    { get; set; }

        [Required]
        public PhoneCondition Condition { get; set; } = PhoneCondition.New;

        [Range(0, 9999)]
        public int Stock { get; set; }

        public bool IsAvailable { get; set; } = true;
        public bool IsFeatured  { get; set; } = false;

        public IEnumerable<Brand> Brands { get; set; } = new List<Brand>();
    }
}
