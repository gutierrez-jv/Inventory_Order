// ViewModel Explanation:
//
// ViewModels are used to transfer data between the Controller and the View (UI).
// They contain only the fields needed for display or form input, not full database structure.
// They also include validation rules using DataAnnotations.

namespace Inventory_Order.ViewModels.Auth
{
    public class LoginResultViewModel
    {
        public int? UserId { get; set; }
        public int? CustomerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
    }
}