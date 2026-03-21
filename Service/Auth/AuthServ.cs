using Inventory_Order.Helpers;
using Inventory_Order.Repository.UserRepository;
using Inventory_Order.ViewModels.Auth;

namespace Inventory_Order.Service.Auth
{
    public class AuthServ : IAuthServ
    {
        private readonly IUserRepo _userRepo;

        public AuthServ(IUserRepo userRepo)
        {
            _userRepo = userRepo;
        }

        public async Task<LoginResultViewModel?> ValidateUserAsync(string username, string password)
        {
            // Hardcoded super admin
            if (username == "admin" && password == "admin123")
            {
                return new LoginResultViewModel
                {
                    Username = "admin",
                    Role = "Admin",
                    UserId = null,
                    CustomerId = null
                };
            }

            // Check database for customer user
            var user = await _userRepo.GetUserByUsernameAsync(username);

            if (user == null)
                return null;

            if (!user.IsActive)
                return null;

            // Verify password
            bool isValidPassword = SecurityHelper.VerifyPassword(password, user.PasswordHash);

            if (!isValidPassword)
                return null;

            // Return login result
            return new LoginResultViewModel
            {
                UserId = user.UserId,
                CustomerId = user.CustomerId,
                Username = user.Username,
                Role = user.Role
            };
        }
    }
}