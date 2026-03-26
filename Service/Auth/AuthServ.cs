using Inventory_Order.Helpers; 
using Inventory_Order.Models.Database; 
using Inventory_Order.Repository.UserRepository; 
using Inventory_Order.ViewModels.Auth; 
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Service.Auth
{
    // Handles authentication and customer registration business logic
    public class AuthServ : IAuthServ
    {
        // Repository used to access user account records
        private readonly IUserRepo _userRepo;

        // Database context used for transaction-based registration
        private readonly InventoryOrderDbContext _dbContext;

        public AuthServ(IUserRepo userRepo, InventoryOrderDbContext dbContext)
        {
            _userRepo = userRepo;
            _dbContext = dbContext;
        }

        // Validates user credentials and returns login data if successful
        public async Task<LoginResultViewModel?> ValidateUserAsync(string username, string password)
        {
            // Allow hardcoded admin login
            if (username == "admin" && password == "admin123")
            {
                return new LoginResultViewModel
                {
                    Username = "admin", // Admin username
                    Role = "Admin", // Admin role
                    FirstName = "Admin", // Display name
                    UserId = null, // No database user ID for hardcoded admin
                    CustomerId = null // No customer ID for admin
                };
            }

            // Retrieve the user record by username
            var user = await _userRepo.GetUserByUsernameAsync(username);

            // Stop if user does not exist or is inactive
            if (user == null || !user.IsActive)
                return null;

            // Verify the entered password against the stored hashed password
            bool isValidPassword = SecurityHelper.VerifyPassword(password, user.PasswordHash);

            // Stop if password is incorrect
            if (!isValidPassword)
                return null;

            // Return login result data for successful login
            return new LoginResultViewModel
            {
                UserId = user.UserId, // User account ID
                CustomerId = user.CustomerId, // Related customer ID
                Username = user.Username, // Username
                Role = user.Role, // User role
                FirstName = user.Customer != null ? user.Customer.FirstName : user.Username // Display first name if available
            };
        }

        // Registers a new customer account and user account
        public async Task<(bool Success, string ErrorMessage)> RegisterCustomerAsync(RegisterViewModel model)
        {
            // Reject duplicate usernames
            if (await _userRepo.UsernameExistsAsync(model.Username))
                return (false, "Username is already taken.");

            // Start a database transaction so both records save together
            using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                // Create a new customer record
                var customer = new CustomerTb
                {
                    FirstName = model.FirstName.Trim(), // Save trimmed first name
                    LastName = model.LastName.Trim(), // Save trimmed last name
                    IsActive = true // New customer starts as active
                };

                // Save customer to database first so CustomerId is generated
                await _dbContext.CustomerTbs.AddAsync(customer);
                await _dbContext.SaveChangesAsync();

                // Create a related user account for the customer
                var user = new UserTb
                {
                    Username = model.Username.Trim(), // Save trimmed username
                    PasswordHash = SecurityHelper.HashPassword(model.Password), // Store hashed password
                    Role = "Customer", // Assign customer role
                    CustomerId = customer.CustomerId, // Link user to the created customer
                    IsActive = true // New user starts as active
                };

                // Save user account to database
                await _dbContext.UserTbs.AddAsync(user);
                await _dbContext.SaveChangesAsync();

                // Commit transaction if both saves succeed
                await transaction.CommitAsync();
                return (true, string.Empty);
            }
            catch
            {
                // Roll back all changes if any step fails
                await transaction.RollbackAsync();
                return (false, "Unable to register customer account at this time.");
            }
        }
    }
}