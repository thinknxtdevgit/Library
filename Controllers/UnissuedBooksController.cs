using lib.Interface;
using lib.Pagination_Helper;
using lib.Service;
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
        [HttpPost("GetUnissuedBooksPaged")]
        public async Task<IActionResult> GetUnissuedBooksPaged([FromBody] PagedRequest request)
        {
            var result =
                await _service
                .GetUnissuedBooksAsyncPages(
                    request.Search,
                    request.PageNumber,
                    request.PageSize);

            return Ok(result);
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

