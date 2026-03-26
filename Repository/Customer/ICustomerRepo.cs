using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.CustomerRepository
{
    // Interface defining methods for managing customers in the database
    public interface ICustomerRepo
    {
        Task<List<CustomerTb>> GetAllCustomersAsync(); // Retrieves a list of all customers from the database
        Task<CustomerTb?> GetCustomerByIdAsync(int id); // Retrieves a specific customer by their ID, returns null if not found
        Task AddCustomerAsync(CustomerTb customer); // Adds a new customer to the database
        Task UpdateCustomerAsync(CustomerTb customer); // Updates an existing customer's information in the database
        Task DeleteCustomerAsync(int id); // Deletes a customer from the database by their ID, or marks them as inactive if they have related orders
    }
}