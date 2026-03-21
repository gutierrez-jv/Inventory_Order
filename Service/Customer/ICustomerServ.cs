using Inventory_Order.Models.Database;

namespace Inventory_Order.Service.Customer
{
    public interface ICustomerServ
    {
        IEnumerable<CustomerTb> GetAllCustomers();
        Task<CustomerTb?> GetCustomerById(int id);
        bool AddCustomer(CustomerTb customer);
        bool UpdateCustomer(CustomerTb customer);
        bool DeleteCustomer(int id);

    }
}
