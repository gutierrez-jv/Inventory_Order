using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.CustomerRepository
{
    public interface ICustomerRepo
    {
        Task<List<CustomerTb>> GetAllCustomersAsync();
        Task<CustomerTb?> GetCustomerByIdAsync(int id);
        Task AddCustomerAsync(CustomerTb customer);
        Task UpdateCustomerAsync(CustomerTb customer);
        Task DeleteCustomerAsync(int id);
    }
}