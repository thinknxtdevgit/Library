using lib.DtoModel.RenewBookDto;
using lib.Interface;
using lib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Controllers
{
    public class RenewBookController : Controller
    {
        private readonly IRenewBookService _renewBookService;

        public RenewBookController(
            IRenewBookService renewBookService)
        {
            _renewBookService = renewBookService;
        }

        [HttpGet("/RenewBook")]
        public IActionResult Index()
        {
            return View("RenewBook");
        }

        [HttpPost]
        [Route("api/book/renew")]
        public async Task<IActionResult> RenewBook(
            [FromBody] RenewBookRequestDto request)
        {
            var result =
                await _renewBookService
                .RenewBookAsync(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }
}