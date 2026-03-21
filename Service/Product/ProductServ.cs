using Inventory_Order.Models.Database;
using Inventory_Order.Repository.ProductRepository;

namespace Inventory_Order.Service.Product
{
    public class ProductServ : IProductServ
    {
        private readonly IProductRepo _productRepo;

        public ProductServ(IProductRepo productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<List<ProductTb>> GetAllProductsAsync()
        {
            var products = await _productRepo.GetAllProductsAsync();
            return products ?? new List<ProductTb>();
        }

        public async Task<ProductTb?> GetProductByIdAsync(int id)
        {
            if (id <= 0)
                return null;

            return await _productRepo.GetProductByIdAsync(id);
        }

        public async Task<bool> CreateProductAsync(ProductTb product)
        {
            if (product == null)
                return false;

            if (string.IsNullOrWhiteSpace(product.Name))
                return false;

            if (string.IsNullOrWhiteSpace(product.Type))
                return false;

            if (string.IsNullOrWhiteSpace(product.Barcode))
                return false;

            if (product.Price <= 0)
                return false;

            if (product.Quantity < 0)
                return false;

            var existingBarcode = await _productRepo.GetProductByBarcodeAsync(product.Barcode);
            if (existingBarcode != null)
                return false;

            await _productRepo.AddProductAsync(product);
            return true;
        }

        public async Task<bool> UpdateProductAsync(ProductTb product)
        {
            if (product == null)
                return false;

            if (product.ProductsId <= 0)
                return false;

            if (string.IsNullOrWhiteSpace(product.Name))
                return false;

            if (string.IsNullOrWhiteSpace(product.Type))
                return false;

            if (string.IsNullOrWhiteSpace(product.Barcode))
                return false;

            if (product.Price <= 0)
                return false;

            if (product.Quantity < 0)
                return false;

            var existingProduct = await _productRepo.GetProductByIdAsync(product.ProductsId);
            if (existingProduct == null)
                return false;

            var duplicateBarcode = await _productRepo.GetProductByBarcodeAsync(product.Barcode);
            if (duplicateBarcode != null && duplicateBarcode.ProductsId != product.ProductsId)
                return false;

            await _productRepo.UpdateProductAsync(product);
            return true;
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            if (id <= 0)
                return false;

            var existingProduct = await _productRepo.GetProductByIdAsync(id);
            if (existingProduct == null)
                return false;

            await _productRepo.DeleteProductAsync(id);
            return true;
        }
    }
}