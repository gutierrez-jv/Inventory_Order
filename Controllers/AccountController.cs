using System.Security.Claims;
using Inventory_Order.Service.Auth;
using Inventory_Order.ViewModels.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthServ _authServ;

        public AccountController(IAuthServ authServ)
        {
            _authServ = authServ;
        }

        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Order");

                if (User.IsInRole("Customer"))
                    return RedirectToAction("MyOrders", "Order");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authServ.ValidateUserAsync(model.Username, model.Password);

            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Invalid username or password.");
                return View(model);
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.Username),
                new Claim(ClaimTypes.Role, result.Role),
                new Claim("FirstName", string.IsNullOrWhiteSpace(result.FirstName) ? result.Username : result.FirstName)
            };

            if (result.UserId.HasValue)
                claims.Add(new Claim("UserId", result.UserId.Value.ToString()));

            if (result.CustomerId.HasValue)
                claims.Add(new Claim("CustomerId", result.CustomerId.Value.ToString()));

            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            if (result.Role == "Admin")
                return RedirectToAction("Index", "Order");

            return RedirectToAction("MyOrders", "Order");
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Order");

                if (User.IsInRole("Customer"))
                    return RedirectToAction("MyOrders", "Order");
            }

            return View(new RegisterViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var result = await _authServ.RegisterCustomerAsync(model);

            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(model);
            }

            TempData["SuccessMessage"] = "Registration successful. You can now sign in as customer.";
            return RedirectToAction(nameof(Login));
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}
