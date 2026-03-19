using Inventory_Order.Models.Database;

namespace Inventory_Order.Repository.CustomerRepository
{
    public interface ICustomerRepo
    {
        Task<List<CustomerTb>> GetAllCustomers();
        Task<CustomerTb?> GetCustomerById(int id);
        void AddCustomer(CustomerTb customer);
        void UpdateCustomer(CustomerTb customer);
        void DeleteCustomer(int id);
    }
}
