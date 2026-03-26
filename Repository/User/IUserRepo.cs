using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.UserRepository
{
    // Defines the contract for user-related database operations, including retrieval of user information by username,
    // checking for existing usernames, and adding new users to the database
    public interface IUserRepo
    {
        Task<UserTb?> GetUserByUsernameAsync(string username);  // Retrieves a user by username
        Task<bool> UsernameExistsAsync(string username); // Checks whether the username already exists
        Task AddUserAsync(UserTb user); // Adds a new user account to the database
    }
}