using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Inventory_Order.Models;

namespace Inventory_Order.Controllers
{
    // This controller serves as the entry point for the application and handles general pages like the home page and privacy policy.
    [Authorize] // All actions in this controller require the user to be authenticated
    public class HomeController : Controller
    {
        // The Index action serves as the default landing page after login.
        public IActionResult Index()
        {
            // If the user is in the "Admin" role, redirect them to the admin order management page.
            if (User.IsInRole("Admin"))
                return RedirectToAction("Index", "Order");

            // If the user is in the "Customer" role, redirect them to their order page.
            if (User.IsInRole("Customer"))
                return RedirectToAction("MyOrders", "Order");

            return RedirectToAction("Login", "Account");
        }

        // The Privacy action displays the privacy policy page. It is accessible to authenticated users.
        public IActionResult Privacy()
        {
            return View(); // this will return the Privacy.cshtml view located but the privacy redirect or link
                           // not on the navbar and there is not much there so
        }

        [AllowAnonymous] // This action can be accessed by unauthenticated users, which is important for error pages.
        // The Error action displays a generic error page when an unhandled exception occurs.
        // It is decorated with ResponseCache attributes to prevent caching of the error page.
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        
        // This method is called when an unhandled exception occurs in the application.
        // It returns a view that displays error details.
        public IActionResult Error() 
        {
            return View(); // returns the Error.cshtml view located in the Views/Home folder but again there is not much there so it is not on the navbar
        }
    }
}