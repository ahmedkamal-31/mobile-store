using System.ComponentModel.DataAnnotations;

namespace MobileStore.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required, MaxLength(100)]
        public string FullName { get; set; } = "";

        [Required, EmailAddress]
        public string Email { get; set; } = "";

        [Required, MinLength(6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = "";

        [Compare("Password")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = "";

        [Required]
        public string Role { get; set; } = "Customer";

        [MaxLength(300)]
        public string? ShopAddress { get; set; }

        [Phone, MaxLength(20)]
        public string? PhoneNumber { get; set; }
    }
}