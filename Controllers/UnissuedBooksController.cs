using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("UnissuedBooks")]
    public class UnissuedBooksController : Controller
    {
        private readonly IUnissuedBooksService _service;
     
        public UnissuedBooksController(IUnissuedBooksService service)
        {
            _service = service;
        }
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("get")]
        public async Task<IActionResult> GetUnissuedBooks(string collegeName)
        {
            var data = await _service.GetUnissuedBooksAsync(collegeName);
            return Ok(data);
        }
        //[HttpGet("export")]
        //public async Task<IActionResult> Export(string collegeName)
        //{
        //    var data = await _service.GetUnissuedBooksAsync(collegeName);
        //    var fileBytes = _service.ExportToExcel(data);

        //    return File(
        //        fileBytes,
        //        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //        "UnissuedBooks.xlsx"
        //    );
        //}
    }
}

