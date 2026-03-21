using Inventory_Order.Models.Database;
using Inventory_Order.Repository.ProductRepository;
using Inventory_Order.Service.Customer;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Service.Product
{
    public class ProductServ : IProductServ
    {
        private readonly IProductRepo _productRepo;
        public ProductServ(ProductRepo productRepo)
        {
            _productRepo = productRepo;
        }
        public bool CreateProduct(ProductTb product)
        {
            if (product == null) return false;
            if (string.IsNullOrWhiteSpace(product.Name)) return false;
            if (string.IsNullOrWhiteSpace(product.Type)) return false;
            if (string.IsNullOrWhiteSpace(product.Barcode)) return false;
            if (product.Price <= 0) return false;
            if (product.Quantity < 0) return false;

            var existingBarcode = _productRepo.GetProductByBarcode(product.Barcode);
            if (existingBarcode != null) return false;

            var existingId = _productRepo.GetProductById(product.ProductsId);
            if (existingId != null) return false;

            product.Stock = product.Quantity > 0;

            _productRepo.AddProduct(product);
            return true;
        }
        public bool DeleteProduct(int id)
        {
            if (id <= 0) return false;

            var existing = _productRepo.GetProductById(id);
            if (existing == null) return false;

            _productRepo.DeleteProduct(id);
            return true;
        }
        public IEnumerable<ProductTb> GetAllProducts()
        {
            return _productRepo.GetAllProducts().Result ?? Enumerable.Empty<ProductTb>();
        }
        public async Task<ProductTb?> GetProductById(int id)
        {
            if (id <= 0) return null;
            return await _productRepo.GetProductById(id);
        }
        public bool UpdateProduct(ProductTb product)
        {
            if (product == null) return false;
            if (string.IsNullOrWhiteSpace(product.Name)) return false;
            if (string.IsNullOrWhiteSpace(product.Type)) return false;
            if (string.IsNullOrWhiteSpace(product.Barcode)) return false;
            if (product.Price <= 0) return false;
            if (product.Quantity < 0) return false;
            if (product.ProductsId <= 0) return false;

            var exisitng = _productRepo.GetProductById(product.ProductsId).Result;
            if (exisitng == null) return false;

            var duplicateBarcode = _productRepo.GetProductByBarcode(product.Barcode).Result;
            if (duplicateBarcode != null && duplicateBarcode.ProductsId != product.ProductsId) return false;

            product.Stock = product.Quantity > 0;

            _productRepo.UpdateProduct(product);
            return true;
        }
    }
}
