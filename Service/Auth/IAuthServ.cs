using Inventory_Order.ViewModels.Auth;

namespace Inventory_Order.Service.Auth
{
    public interface IAuthServ
    {
        Task<LoginResultViewModel?> ValidateUserAsync(string username, string password);
        Task<(bool Success, string ErrorMessage)> RegisterCustomerAsync(RegisterViewModel model);
    }
}
