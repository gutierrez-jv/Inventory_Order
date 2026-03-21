namespace Inventory_Order.ViewModels.Auth
{
    public class LoginResultViewModel
    {
        public int? UserId { get; set; }
        public int? CustomerId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}