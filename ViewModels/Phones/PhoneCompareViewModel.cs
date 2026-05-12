using MobileStore.Models;

namespace MobileStore.ViewModels.Phones
{
    public class CompareViewModel
    {
        public Phone? PhoneA { get; set; }
        public Phone? PhoneB { get; set; }
        public IEnumerable<Phone> AllPhones { get; set; } = new List<Phone>();
    }
}