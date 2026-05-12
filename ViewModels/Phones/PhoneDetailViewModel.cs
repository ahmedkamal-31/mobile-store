using MobileStore.Models;

namespace MobileStore.ViewModels.Phones
{
    public class PhoneDetailViewModel
    {
        public Phone Phone { get; set; } = null!;
        public IEnumerable<Phone> SimilarPhones { get; set; } = new List<Phone>();
        public bool IsInCart { get; set; }
    }
}