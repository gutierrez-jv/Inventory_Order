using Inventory_Order.Models.Database; 
using Microsoft.EntityFrameworkCore; 

namespace Inventory_Order.Repository.UserRepository
{
    // Handles direct database operations for user accounts
    public class UserRepo : IUserRepo
    {
        // Database context used to access user data
        private readonly InventoryOrderDbContext _context;

        public UserRepo(InventoryOrderDbContext context)
        {
            _context = context;
        }

        // Retrieves a user by username together with related customer data
        public async Task<UserTb?> GetUserByUsernameAsync(string username)
        {
            return await _context.UserTbs
                .Include(u => u.Customer) // Load related customer record
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        // Checks if a username already exists in the database
        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.UserTbs
                .AnyAsync(u => u.Username == username);
        }

        // Adds a new user account to the database
        public async Task AddUserAsync(UserTb user)
        {
            await _context.UserTbs.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}