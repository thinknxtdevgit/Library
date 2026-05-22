using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    public class DashboardController : Controller
    {
        [HttpGet("/Dashboard")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
