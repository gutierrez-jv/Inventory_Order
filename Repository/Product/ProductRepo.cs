using Inventory_Order.Models.Database;
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
        }

        public void DeleteProduct(int id)
        {
            var ex = _context.ProductTbs.Find(id);
            if (ex != null) _context.ProductTbs.Remove(ex);
            _context.SaveChanges();
        }

        public List<ProductTb> GetAllProducts()
        {
            return _context.ProductTbs.ToList();
        }

        public ProductTb? GetProductByBarcode(string barcode)
        {
            _context.ProductTbs.Where(p => p.Barcode == barcode).FirstOrDefault();
            return null;
        }

        public ProductTb? GetProductById(int id)
        {
            _context.ProductTbs.Where(p => p.ProductsId == id).FirstOrDefault();
            return null;
        }
        public void UpdateProduct(ProductTb product)
        {
            _context.ProductTbs.Update(product);
        }
    }
}
