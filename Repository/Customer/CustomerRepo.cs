using Inventory_Order.Models.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Inventory_Order.Repository.CustomerRepository
{
    public class CustomerRepo : ICustomerRepo
    {
        private readonly InventoryOrderDbContext _context;
        public CustomerRepo(InventoryOrderDbContext context)
        {
            _context = context;
        }
        public void AddCustomer(CustomerTb customer)
        {
            _context.CustomerTbs.Add(customer);
            _context.SaveChanges();
        }

        public void DeleteCustomer(int id)
        {
            var ex = _context.CustomerTbs.Find(id);
            if (ex != null) _context.CustomerTbs.Remove(ex);
            _context.SaveChanges();
        }

        public async Task<List<CustomerTb>> GetAllCustomers()
        {
            return await _context.CustomerTbs.ToListAsync();
        }

        public async Task<CustomerTb?> GetCustomerById(int id)
        {
            return await _context.CustomerTbs.FindAsync(id);
        }

        public void UpdateCustomer(CustomerTb customer)
        {
            _context.CustomerTbs.Update(customer);
            _context.SaveChanges();
        }
    }
}
