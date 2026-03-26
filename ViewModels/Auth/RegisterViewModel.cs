using System.ComponentModel.DataAnnotations;

namespace Inventory_Order.ViewModels.Auth
{
    public class RegisterViewModel
    {
        [Required]
        [StringLength(50)] // Maximum length of 50 characters
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [StringLength(50, MinimumLength = 4)] // Username must be between 4 and 50 characters
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)] // Masks input in forms
        [StringLength(100, MinimumLength = 6)] // Password must be between 6 and 100 characters
        public string Password { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")] // Ensures ConfirmPassword matches Password
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
