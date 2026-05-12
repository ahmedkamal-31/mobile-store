using MobileStore.Models;

namespace MobileStore.ViewModels.Phones
{
    public class RecommendationViewModel
    {
        public Phone BasePhone { get; set; } = null!;
        public IEnumerable<Phone> Recommended { get; set; } = new List<Phone>();
    }
}