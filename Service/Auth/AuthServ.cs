using Inventory_Order.Helpers;
using Inventory_Order.Models.Database;
using Inventory_Order.Repository.UserRepository;
using Inventory_Order.ViewModels.Auth;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Service.Auth
{
    public class AuthServ : IAuthServ
    {
        private readonly IUserRepo _userRepo;
        private readonly InventoryOrderDbContext _dbContext;

        public AuthServ(IUserRepo userRepo, InventoryOrderDbContext dbContext)
        {
            _userRepo = userRepo;
            _dbContext = dbContext;
        }

        public async Task<LoginResultViewModel?> ValidateUserAsync(string username, string password)
        {
            if (username == "admin" && password == "admin123")
            {
                return new LoginResultViewModel
                {
                    Username = "admin",
                    Role = "Admin",
                    FirstName = "Admin",
                    UserId = null,
                    CustomerId = null
                };
            }

            var user = await _userRepo.GetUserByUsernameAsync(username);

            if (user == null || !user.IsActive)
                return null;

            bool isValidPassword = SecurityHelper.VerifyPassword(password, user.PasswordHash);

            if (!isValidPassword)
                return null;

            return new LoginResultViewModel
            {
                UserId = user.UserId,
                CustomerId = user.CustomerId,
                Username = user.Username,
                Role = user.Role,
                FirstName = user.Customer != null ? user.Customer.FirstName : user.Username
            };
        }

        public async Task<(bool Success, string ErrorMessage)> RegisterCustomerAsync(RegisterViewModel model)
        {
            if (await _userRepo.UsernameExistsAsync(model.Username))
                return (false, "Username is already taken.");

            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var customer = new CustomerTb
                {
                    FirstName = model.FirstName.Trim(),
                    LastName = model.LastName.Trim(),
                    IsActive = true
                };

                await _dbContext.CustomerTbs.AddAsync(customer);
                await _dbContext.SaveChangesAsync();

                var user = new UserTb
                {
                    Username = model.Username.Trim(),
                    PasswordHash = SecurityHelper.HashPassword(model.Password),
                    Role = "Customer",
                    CustomerId = customer.CustomerId,
                    IsActive = true
                };

                await _dbContext.UserTbs.AddAsync(user);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, string.Empty);
            }
            catch
            {
                await transaction.RollbackAsync();
                return (false, "Unable to register customer account at this time.");
            }
        }
    }
}
