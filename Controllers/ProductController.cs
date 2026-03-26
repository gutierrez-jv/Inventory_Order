using Inventory_Order.Models.Database;
using Inventory_Order.Service.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    [Authorize(Roles = "Admin,Customer")]
    public class ProductController : Controller
    {
        private readonly IProductServ _productServ;

        public ProductController(IProductServ productServ)
        {
            _productServ = productServ;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productServ.GetAllProductsAsync();
            return View(products);
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ProductTb product)
        {
            if (!ModelState.IsValid)
                return View(product);

            bool success;
            try
            {
                success = await _productServ.CreateProductAsync(product);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while creating the product.");
                return View(product);
            }

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create product. Please verify required fields and barcode uniqueness.");
                return View(product);
            }

            TempData["SuccessMessage"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _productServ.GetProductByIdAsync(id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(ProductTb product)
        {
            if (!ModelState.IsValid)
                return View(product);

            bool success;
            try
            {
                success = await _productServ.UpdateProductAsync(product);
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "A system error occurred while updating the product.");
                return View(product);
            }

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update product. Please verify required fields and barcode uniqueness.");
                return View(product);
            }

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool success;
            try
            {
                success = await _productServ.DeleteProductAsync(id);
            }
            catch
            {
                TempData["ErrorMessage"] = "A system error occurred while deleting the product.";
                return RedirectToAction(nameof(Index));
            }

            if (!success)
                TempData["ErrorMessage"] = "Unable to delete product. It may already be used in existing orders.";
            else
                TempData["SuccessMessage"] = "Product deleted successfully.";

            return RedirectToAction(nameof(Index));
        }
    }
}
