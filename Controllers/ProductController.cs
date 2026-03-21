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

            var success = await _productServ.CreateProductAsync(product);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create product.");
                return View(product);
            }

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

            var success = await _productServ.UpdateProductAsync(product);

            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update product.");
                return View(product);
            }

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var success = await _productServ.DeleteProductAsync(id);

            if (!success)
                TempData["ErrorMessage"] = "Unable to delete product. It may already be used in existing orders.";

            return RedirectToAction(nameof(Index));
        }
    }
}
