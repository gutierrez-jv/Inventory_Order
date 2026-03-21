using Inventory_Order.Models.Database;
using Inventory_Order.Repository.CustomerRepository;

namespace Inventory_Order.Service.Customer
{
    public class CustomerServ : ICustomerServ
    {
        private readonly ICustomerRepo _customerRepo;

        public CustomerServ(ICustomerRepo customerRepo)
        {
            _customerRepo = customerRepo;
        }

        public async Task<List<CustomerTb>> GetAllCustomersAsync()
        {
            var customers = await _customerRepo.GetAllCustomersAsync();
            return customers ?? new List<CustomerTb>();
        }

        public async Task<CustomerTb?> GetCustomerByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _customerRepo.GetCustomerByIdAsync(id);
        }

        public async Task<bool> AddCustomerAsync(CustomerTb customer)
        {
            if (customer == null)
                return false;

            if (string.IsNullOrWhiteSpace(customer.FirstName) ||
                string.IsNullOrWhiteSpace(customer.LastName))
                return false;

            await _customerRepo.AddCustomerAsync(customer);
            return true;
        }

        public async Task<bool> UpdateCustomerAsync(CustomerTb customer)
        {
            if (customer == null)
                return false;

            if (customer.CustomerId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(customer.FirstName) ||
                string.IsNullOrWhiteSpace(customer.LastName))
                return false;

            var existingCustomer = await _customerRepo.GetCustomerByIdAsync(customer.CustomerId);
            if (existingCustomer == null)
                return false;

            await _customerRepo.UpdateCustomerAsync(customer);
            return true;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            if (id <= 0)
                return false;

            var existingCustomer = await _customerRepo.GetCustomerByIdAsync(id);
            if (existingCustomer == null)
                return false;

            await _customerRepo.DeleteCustomerAsync(id);
            return true;
        }
    }
}