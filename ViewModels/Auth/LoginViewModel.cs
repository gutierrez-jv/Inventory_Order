using System.ComponentModel.DataAnnotations;

namespace Inventory_Order.ViewModels.Auth
{
    public class LoginViewModel
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        [DataType(DataType.Password)] // Masks input in forms
        public string Password { get; set; } = string.Empty;
    }
}