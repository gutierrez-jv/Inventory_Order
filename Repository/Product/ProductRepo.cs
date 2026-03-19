using Inventory_Order.Models.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Inventory_Order.Repository.ProductRepository
{
    public class ProductRepo : IProductRepo
    {
        private readonly InventoryOrderDbContext _context;

        public ProductRepo(InventoryOrderDbContext context)
        {
            _context = context;
        }
        public void AddProduct(ProductTb product)
        {
            _context.ProductTbs.Add(product);
            _context.SaveChanges();
        }

        public void DeleteProduct(int id)
        {
            var ex = _context.ProductTbs.Find(id);
            if (ex != null) _context.ProductTbs.Remove(ex);
            _context.SaveChanges();
        }

        public async Task<List<ProductTb>> GetAllProducts()
        {
            return await _context.ProductTbs.ToListAsync();
        }

        public async Task<ProductTb?> GetProductByBarcode(string barcode)
        {
            return await _context.ProductTbs.FirstOrDefaultAsync(p => p.Barcode == barcode);
        }

        public async Task<ProductTb?> GetProductById(int id)
        {
            return await _context.ProductTbs.FindAsync(id);
        }

        public void UpdateProduct(ProductTb product)
        {
            _context.ProductTbs.Update(product);
            _context.SaveChanges();
        }
    }
}
