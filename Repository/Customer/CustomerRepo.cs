using Microsoft.AspNetCore.Mvc;

namespace Inventory_Order.Repository.CustomerRepository
{
    public class CustomerRepo : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
