using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    public class SettingsController : Controller
    {
        [HttpGet("/Settings")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
