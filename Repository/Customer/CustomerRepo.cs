using Inventory_Order.Models.Database;
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

        public async Task<List<CustomerTb>> GetAllCustomersAsync()
        {
            return await _context.CustomerTbs.ToListAsync();
        }

        public async Task<CustomerTb?> GetCustomerByIdAsync(int id)
        {
            return await _context.CustomerTbs.FindAsync(id);
        }

        public async Task AddCustomerAsync(CustomerTb customer)
        {
            await _context.CustomerTbs.AddAsync(customer);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCustomerAsync(CustomerTb customer)
        {
            _context.CustomerTbs.Update(customer);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCustomerAsync(int id)
        {
            var existingCustomer = await _context.CustomerTbs.FindAsync(id);
            if (existingCustomer != null)
            {
                _context.CustomerTbs.Remove(existingCustomer);
                await _context.SaveChangesAsync();
            }
        }
    }
}