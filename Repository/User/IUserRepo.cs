using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.UserRepository
{
    public interface IUserRepo
    {
        Task<UserTb?> GetUserByUsernameAsync(string username);
        Task<bool> UsernameExistsAsync(string username);
        Task AddUserAsync(UserTb user);
    }
}