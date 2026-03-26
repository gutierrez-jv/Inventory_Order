using Inventory_Order.Models.Database; 
using Inventory_Order.Repository.ProductRepository; 

namespace Inventory_Order.Service.Product
{
    // Handles product-related business logic before calling the repository
    public class ProductServ : IProductServ
    {
        // Repository used to access product records
        private readonly IProductRepo _productRepo;

        public ProductServ(IProductRepo productRepo)
        {
            _productRepo = productRepo;
        }

        // Retrieves all products and returns an empty list if null
        public async Task<List<ProductTb>> GetAllProductsAsync()
        {
            var products = await _productRepo.GetAllProductsAsync();
            return products ?? new List<ProductTb>(); // Return empty list if repository returns null
        }

        // Retrieves a product by ID only if the ID is valid
        public async Task<ProductTb?> GetProductByIdAsync(int id)
        {
            if (id <= 0) // Reject invalid IDs
                return null;

            return await _productRepo.GetProductByIdAsync(id);
        }

        // Validates and creates a new product
        public async Task<bool> CreateProductAsync(ProductTb product)
        {
            if (product == null) // Reject null input
                return false;

            if (string.IsNullOrWhiteSpace(product.Name)) // Product name is required
                return false;

            if (string.IsNullOrWhiteSpace(product.Type)) // Product type is required
                return false;

            if (string.IsNullOrWhiteSpace(product.Barcode)) // Barcode is required
                return false;

            if (product.Price <= 0) // Price must be greater than zero
                return false;

            if (product.Quantity < 0) // Quantity cannot be negative
                return false;

            // Reject duplicate barcodes
            var existingBarcode = await _productRepo.GetProductByBarcodeAsync(product.Barcode);
            if (existingBarcode != null)
                return false;

            await _productRepo.AddProductAsync(product); // Save valid product
            return true;
        }

        // Validates and updates an existing product
        public async Task<bool> UpdateProductAsync(ProductTb product)
        {
            if (product == null) // Reject null input
                return false;

            if (product.ProductsId <= 0) // Reject invalid product ID
                return false;

            if (string.IsNullOrWhiteSpace(product.Name)) // Product name is required
                return false;

            if (string.IsNullOrWhiteSpace(product.Type)) // Product type is required
                return false;

            if (string.IsNullOrWhiteSpace(product.Barcode)) // Barcode is required
                return false;

            if (product.Price <= 0) // Price must be greater than zero
                return false;

            if (product.Quantity < 0) // Quantity cannot be negative
                return false;

            // Make sure the product exists before updating
            var existingProduct = await _productRepo.GetProductByIdAsync(product.ProductsId);
            if (existingProduct == null)
                return false;

            // Reject duplicate barcodes belonging to another product
            var duplicateBarcode = await _productRepo.GetProductByBarcodeAsync(product.Barcode);
            if (duplicateBarcode != null && duplicateBarcode.ProductsId != product.ProductsId)
                return false;

            await _productRepo.UpdateProductAsync(product); // Update valid existing product
            return true;
        }

        // Validates and deletes an existing product
        public async Task<bool> DeleteProductAsync(int id)
        {
            if (id <= 0) // Reject invalid IDs
                return false;

            // Make sure the product exists before deleting
            var existingProduct = await _productRepo.GetProductByIdAsync(id);
            if (existingProduct == null)
                return false;

            // Prevent deletion if the product is already used in orders
            var hasOrders = await _productRepo.HasOrderItemsAsync(id);
            if (hasOrders)
                return false;

            await _productRepo.DeleteProductAsync(id); // Delete valid unused product
            return true;
        }
    }
}