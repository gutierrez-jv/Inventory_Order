using Inventory_Order.ViewModels.Account;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    public class AccountController : Controller
    {
        private readonly IConfiguration _configuration;
        private const string AdminSessionKey = "IsAdminLoggedIn";

        public AccountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (HttpContext.Session.GetString(AdminSessionKey) == "true")
            {
                return RedirectToAction("Index", "Order");
            }

            return View(new LoginViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var adminUsername = _configuration["AdminLogin:Username"] ?? "admin";
            var adminPassword = _configuration["AdminLogin:Password"] ?? "admin123";

            if (model.Username == adminUsername && model.Password == adminPassword)
            {
                HttpContext.Session.SetString(AdminSessionKey, "true");
                HttpContext.Session.SetString("AdminUsername", model.Username);
                return RedirectToAction("Index", "Order");
            }

            ModelState.AddModelError(string.Empty, "Invalid administrator credentials.");
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction(nameof(Login));
        }
    }
}
