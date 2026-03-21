using Inventory_Order.Models.Database;
using Inventory_Order.Repository.CustomerRepository;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Service.Customer
{
    public class CustomerServ : ICustomerServ
    {
        private readonly ICustomerRepo _customerRepo;
        public CustomerServ(ICustomerRepo customerRepo)
        {
            _customerRepo = customerRepo;
        }

        public bool AddCustomer(CustomerTb customer)
        {
            if (customer == null) return false;
            if (string.IsNullOrEmpty(customer.FirstName) 
                || string.IsNullOrEmpty(customer.LastName)) return false;

            _customerRepo.AddCustomer(customer);
            return true;
        }

        public bool DeleteCustomer(int id)
        {
            if (id <= 0) return false;

            var existingCustomer = _customerRepo.GetCustomerById(id);
            if (existingCustomer == null) return false;

            _customerRepo.DeleteCustomer(id);
            return true;
        }

        public IEnumerable<CustomerTb> GetAllCustomers()
        {
            return _customerRepo.GetAllCustomers().Result ?? Enumerable.Empty<CustomerTb>();
        }

        public async Task<CustomerTb?> GetCustomerById(int id)
        {
            if (id <= 0) return null;
            return await _customerRepo.GetCustomerById(id);
        }

        public bool UpdateCustomer(CustomerTb customer)
        {
            if (customer == null) return false;
            if (customer.CustomerId <= 0) return false;
            if (string.IsNullOrEmpty(customer.FirstName) 
                || string.IsNullOrEmpty(customer.LastName)) return false;

            var existing = _customerRepo.GetCustomerById(customer.CustomerId);
            if (existing == null) return false;

            _customerRepo.UpdateCustomer(customer);
            return true;
        }
    }
}
