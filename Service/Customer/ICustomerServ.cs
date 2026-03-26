using Inventory_Order.Models.Database; 
namespace Inventory_Order.Service.Customer
{
    // Defines customer-related business logic operations
    public interface ICustomerServ
    {
        Task<List<CustomerTb>> GetAllCustomersAsync(); // Retrieves all customers
        Task<CustomerTb?> GetCustomerByIdAsync(int id);  // Retrieves a specific customer by ID
        Task<bool> AddCustomerAsync(CustomerTb customer);  // Validates and adds a new customer
        Task<bool> UpdateCustomerAsync(CustomerTb customer); // Validates and updates an existing customer
        Task<bool> DeleteCustomerAsync(int id); // Validates and deletes an existing customer
    }
}