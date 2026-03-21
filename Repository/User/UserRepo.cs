using Inventory_Order.Models.Database;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Repository.UserRepository
{
    public class UserRepo : IUserRepo
    {
        private readonly InventoryOrderDbContext _context;

        public UserRepo(InventoryOrderDbContext context)
        {
            _context = context;
        }

        public async Task<UserTb?> GetUserByUsernameAsync(string username)
        {
            return await _context.UserTbs
                .Include(u => u.Customer)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public async Task<bool> UsernameExistsAsync(string username)
        {
            return await _context.UserTbs
                .AnyAsync(u => u.Username == username);
        }

        public async Task AddUserAsync(UserTb user)
        {
            await _context.UserTbs.AddAsync(user);
            await _context.SaveChangesAsync();
        }
    }
}