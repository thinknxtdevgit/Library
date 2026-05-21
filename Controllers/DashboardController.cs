using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
