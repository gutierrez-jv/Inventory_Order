using Inventory_Order.ViewModels.Auth;

namespace Inventory_Order.Service.Auth
{
    // This service interface defines methods for user authentication and registration.
    public interface IAuthServ
    {
        // Validates login credentials and returns login result data 
        Task<LoginResultViewModel?> ValidateUserAsync(string username, string password);
        
        // Validates login credentials and returns login result data
        Task<(bool Success, string ErrorMessage)> RegisterCustomerAsync(RegisterViewModel model);     
    }
}
