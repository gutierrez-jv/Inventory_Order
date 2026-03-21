using Inventory_Order.Models.Database;
using Inventory_Order.Service.Product;
using Inventory_Order.ViewModels.Product;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductServ _productService;

        public ProductController(IProductServ productService)
        {
            _productService = productService;
        }

        public IActionResult Index()
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            var products = _productService.GetAllProducts();
            return View(products);
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            return View(new ProductFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(ProductFormViewModel model)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var product = new ProductTb
            {
                ProductsId = model.ProductsId,
                Name = model.Name.Trim(),
                Type = model.Type.Trim(),
                Quantity = model.Quantity,
                Price = model.Price,
                Barcode = model.Barcode.Trim(),
                Stock = model.Quantity > 0
            };

            var success = _productService.CreateProduct(product);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to create product. Check duplicate Product ID/Barcode and required values.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Product created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            var product = _productService.GetProductById(id).Result;
            if (product == null) return NotFound();

            var model = new ProductFormViewModel
            {
                ProductsId = product.ProductsId,
                Name = product.Name,
                Type = product.Type,
                Quantity = product.Quantity,
                Price = product.Price,
                Barcode = product.Barcode
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(ProductFormViewModel model)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            if (!ModelState.IsValid) return View(model);

            var product = new ProductTb
            {
                ProductsId = model.ProductsId,
                Name = model.Name.Trim(),
                Type = model.Type.Trim(),
                Quantity = model.Quantity,
                Price = model.Price,
                Barcode = model.Barcode.Trim(),
                Stock = model.Quantity > 0
            };

            var success = _productService.UpdateProduct(product);
            if (!success)
            {
                ModelState.AddModelError(string.Empty, "Unable to update product. Ensure product exists and barcode is unique.");
                return View(model);
            }

            TempData["SuccessMessage"] = "Product updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            if (!IsAdminAuthenticated()) return RedirectToAction("Login", "Account");

            var success = _productService.DeleteProduct(id);
            TempData[success ? "SuccessMessage" : "ErrorMessage"] = success
                ? "Product deleted successfully."
                : "Unable to delete product.";

            return RedirectToAction(nameof(Index));
        }

        private bool IsAdminAuthenticated()
        {
            return HttpContext.Session.GetString("IsAdminLoggedIn") == "true";
        }
    }
}
