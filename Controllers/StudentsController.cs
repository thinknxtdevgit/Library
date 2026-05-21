using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    public class StudentsController : Controller
    {
        [HttpGet("/Students")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("/Students/Create")]
        public IActionResult Create()
        {
            return View();
        }
    }
}
