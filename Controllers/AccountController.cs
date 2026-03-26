using System.Security.Claims; // Used for storing user identity information (username, role, etc.)
using Inventory_Order.Service.Auth; // Service layer for authentication logic
using Inventory_Order.ViewModels.Auth; // ViewModels for Login and Register
using Microsoft.AspNetCore.Authentication; // Authentication methods
using Microsoft.AspNetCore.Authentication.Cookies; // Cookie-based authentication
using Microsoft.AspNetCore.Mvc; // MVC Controller

namespace Inventory_Order.Controllers
{
    // This controller handles user authentication (Login, Register, Logout)
    public class AccountController : Controller
    {
        // Service dependency (used to validate and register users)
        private readonly IAuthServ _authServ;

        public AccountController(IAuthServ authServ)
        {
            _authServ = authServ;
        }

        // [HttpGet]
        // This method responds to an HTTP GET request.
        // GET is used to DISPLAY or RETRIEVE data.
        // In this case, it displays the Login page.
        //
        // Example:
        // When the user navigates to /Account/Login,
        // this method runs and returns the Login view.
        //
        // Key idea:
        // GET = show/display page
        [HttpGet]
        public IActionResult Login()
        {
            // If user is already logged in
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                // Redirect based on role
                if (User.IsInRole("Admin"))
                    // redirect to admin order management page (To OrderController, Order action )
                    return RedirectToAction("Index", "Order");

                if (User.IsInRole("Customer"))
                    // redirect to customer's order page (To OrderController, MyOrders action )
                    return RedirectToAction("MyOrders", "Order");
            }

            // If not logged in, show login page
            return View();
        }

        // [HttpPost]
        // This method responds to an HTTP POST request.
        // POST is used to SEND or PROCESS data.
        //
        // Example:
        // When the user submits the login form,
        // this method runs and processes the username and password.
        //
        // Key idea:
        // POST = submit/process data
        [HttpPost]
        [ValidateAntiForgeryToken] // Security: prevents CSRF attacks or Cross-Site Request Forgery (malicious form submissions from other sites)
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            // Check if input is valid
            if (!ModelState.IsValid)
                return View(model);

            LoginResultViewModel? result; // Result from authentication service (null if login failed)

            try
            {
                // Call service to validate user credentials
                result = await _authServ.ValidateUserAsync(model.Username, model.Password);
            }
            catch
            {
                // If system error occurs (e.g. DB issue)
                ModelState.AddModelError(string.Empty, "Unable to sign in right now. Please try again in a moment.");
                return View(model);
            }

            // If login failed
            if (result == null)
            {
                // Generic error message (don't reveal if username or password was wrong)
                ModelState.AddModelError(string.Empty, "Invalid username or password."); 
                return View(model);
            }

            // Create claims (user identity stored in cookie)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, result.Username), // Username
                new Claim(ClaimTypes.Role, result.Role),     // Role (Admin or Customer)

                // Store first name for personalized greetings (if available, otherwise use username)
                new Claim("FirstName", string.IsNullOrWhiteSpace(result.FirstName) ? result.Username : result.FirstName) 
            };

            // Add UserId if available
            if (result.UserId.HasValue)
                claims.Add(new Claim("UserId", result.UserId.Value.ToString()));

            // Add CustomerId if available
            if (result.CustomerId.HasValue)
                claims.Add(new Claim("CustomerId", result.CustomerId.Value.ToString()));

            // Create identity and principal (for authentication cookie)
            var identity = new ClaimsIdentity(
                claims,
                CookieAuthenticationDefaults.AuthenticationScheme);

            // ClaimsPrincipal represents the authenticated user (contains their identity and claims)
            var principal = new ClaimsPrincipal(identity);

            // Sign in the user (creates cookie session)
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                principal);

            // Success message (used by site.js)
            TempData["SuccessMessage"] = "Login successful. Welcome back, " +
                (string.IsNullOrWhiteSpace(result.FirstName) ? result.Username : result.FirstName) + "!";

            // Redirect based on role
            if (result.Role == "Admin") return RedirectToAction("Index", "Order");
            // Default to customer orders page
            return RedirectToAction("MyOrders", "Order");
        }

        // =========================
        // REGISTER (GET)
        // =========================

        // [HttpGet]
        // Displays the registration page.
        // GET = display page
        [HttpGet]
        public IActionResult Register()
        {
            // Prevent logged-in users from accessing register page
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Index", "Order");

                if (User.IsInRole("Customer"))
                    return RedirectToAction("MyOrders", "Order");
            }

            return View(new RegisterViewModel());
        }

        // =========================
        // REGISTER (POST)
        // =========================

        // [HttpPost]
        // Processes registration form submission.
        // POST = submit/process data
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            // Validate input
            if (!ModelState.IsValid)
                return View(model);

            // Result from registration service (Success = true if registration succeeded, otherwise false with error message)
            (bool Success, string ErrorMessage) result; 

            try
            {
                // Call service to register new customer
                result = await _authServ.RegisterCustomerAsync(model);
            }
            catch
            {
                // If system error occurs (e.g. DB issue)
                ModelState.AddModelError(string.Empty, "Unable to register right now. Please try again later.");
                return View(model);
            }

            // If registration failed
            if (!result.Success)
            {
                // Show error message from service (e.g. username already taken)
                ModelState.AddModelError(string.Empty, result.ErrorMessage);
                return View(model);
            }

            // Success message
            TempData["SuccessMessage"] = "Registration successful. You can now sign in as customer.";

            // Redirect to login page
            return RedirectToAction(nameof(Login));
        }

        // =========================
        // LOGOUT
        // =========================

        public async Task<IActionResult> Logout()
        {
            // Removes authentication cookie (logs out user)
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            return RedirectToAction("Login", "Account");
        }

        // =========================
        // ACCESS DENIED
        // =========================

        public IActionResult AccessDenied()
        {
            // Page shown when user tries to access unauthorized content
            return View();
        }
    }
}