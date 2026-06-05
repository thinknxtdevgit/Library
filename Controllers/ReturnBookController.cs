using lib.DtoModel.ReturnBookDto;
using lib.Interface;
using lib.Models.ReturnBook;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Controllers
{
    public class ReturnBookController : Controller
    {

        private readonly IReturnBookService _returnBookService;

        public ReturnBookController(
            IReturnBookService returnBookService)
        {
            _returnBookService = returnBookService;
        }

        [HttpGet("/ReceiveBook")]
        public IActionResult Index()
        {
            return View("ReturnBook");
        }

        [HttpPost("/ReceiveBook")]
        public async Task<IActionResult> ReceiveBook(
            [FromBody] ReceiveBookRequestDto request)
        {
            string collegeName =
            HttpContext.Session.GetString("CollegeName");
            var result =
                await _returnBookService.ReceiveBookAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }

}