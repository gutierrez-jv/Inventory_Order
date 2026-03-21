using Inventory_Order.Models.Database;

namespace Inventory_Order.Service.Customer
{
    public interface ICustomerServ
    {
        Task<List<CustomerTb>> GetAllCustomersAsync();
        Task<CustomerTb?> GetCustomerByIdAsync(int id);
        Task<bool> AddCustomerAsync(CustomerTb customer);
        Task<bool> UpdateCustomerAsync(CustomerTb customer);
        Task<bool> DeleteCustomerAsync(int id);
    }
}