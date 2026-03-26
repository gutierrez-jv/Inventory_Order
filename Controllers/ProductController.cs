using Inventory_Order.Models.Database; 
using Inventory_Order.Service.Product; 
using Microsoft.AspNetCore.Authorization; 
using Microsoft.AspNetCore.Mvc; 

namespace Inventory_Order.Controllers
{
    // Allows both Admin and Customer to view products, but restricts certain action
    [Authorize(Roles = "Admin,Customer")]
    public class ProductController : Controller
    {
        // Service for handling product-related operations
        private readonly IProductServ _productServ;

        // Constructor (Dependency Injection)
        public ProductController(IProductServ productServ)
        {
            _productServ = productServ;
        }

        // Displays all products
        public async Task<IActionResult> Index()
        {
            var products = await _productServ.GetAllProductsAsync(); // Get all products from service
            return View(products); // Pass products to view
        }

        // Only Admin can access create page
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View(); // Show create product form
        }

        // Only Admin can submit product creation
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductTb product)
        {
            if (!ModelState.IsValid) // Check if form input is valid
                return View(product); // Return form with validation errors

            bool success;
            try
            {
                success = await _productServ.CreateProductAsync(product); // Create product using service
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while creating the product.");
                return View(product);
            }

            if (!success) // If creation failed
            {
                ModelState.AddModelError(string.Empty, "Unable to create product. Please verify required fields and barcode uniqueness.");
                return View(product);
            }

            TempData["SuccessMessage"] = "Product created successfully."; // Store success message
            return RedirectToAction(nameof(Index)); // Redirect to product list
        }

        // Only Admin can edit products
        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productServ.GetProductByIdAsync(id); // Get product by ID

            if (product == null)
                return NotFound(); // Return 404 if product not found

            return View(product); // Send product data to edit form
        }

        // Only Admin can submit product updates
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductTb product)
        {
            if (!ModelState.IsValid) // Validate input
                return View(product); // Return form with validation errors

            bool success;
            try
            {
                success = await _productServ.UpdateProductAsync(product); // Update product using service
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while updating the product.");
                return View(product);
            }

            if (!success) // If update failed
            {
                ModelState.AddModelError(string.Empty, "Unable to update product. Please verify required fields and barcode uniqueness.");
                return View(product);
            }

            TempData["SuccessMessage"] = "Product updated successfully."; // Success message
            return RedirectToAction(nameof(Index)); // Redirect to product list
        }

        // Only Admin can delete products
        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool success;
            try
            {
                success = await _productServ.DeleteProductAsync(id); // Delete product using service
            }
            catch
            {
                TempData["ErrorMessage"] = "A system error occurred while deleting the product.";
                return RedirectToAction(nameof(Index));
            }

            if (!success) // If delete failed (e.g., product used in orders)
                TempData["ErrorMessage"] = "Unable to delete product. It may already be used in existing orders.";
            else
                TempData["SuccessMessage"] = "Product deleted successfully.";

            return RedirectToAction(nameof(Index)); // Redirect to product list
        }
    }
}