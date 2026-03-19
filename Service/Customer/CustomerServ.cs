using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Service.Customer
{
    public class CustomerServ : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
