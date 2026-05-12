using MobileStore.Models;

namespace MobileStore.ViewModels.Phones
{
    public class PhoneListViewModel
    {
        public IEnumerable<Phone> Phones { get; set; } = new List<Phone>();
        public IEnumerable<Brand> Brands { get; set; } = new List<Brand>();

        public int? FilterBrandId { get; set; }
        public string? SearchQuery { get; set; }
        public string SortBy { get; set; } = "newest";
        public PhoneCondition? FilterCondition { get; set; }

        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }

        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
    }
}