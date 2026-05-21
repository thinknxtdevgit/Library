using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    public class BooksController : Controller
    {
        [HttpGet("/Books")]
        public IActionResult Index()
        {
            // Placeholder: Typically this would list books. 
            // For now, redirecting to Create or returning a stub.
            return View("~/Views/AddStockRegister/AddStockRegister.cshtml");
        }

        [HttpGet("/Books/Create")]
        public IActionResult Create()
        {
            return View("~/Views/AddStockRegister/AddStockRegister.cshtml");
        }
    }
}
