
using lib.DtoModel.AddStockBookDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace lib.Controllers
{
    [ApiController]
    [Route("")]
    public class AddStockRegisterController : Controller
    {
        private readonly IStockRegisterService _service;

        public AddStockRegisterController(
            IStockRegisterService service)
        {
            _service = service;
        }

        [HttpGet("/AddStockRegister")]
        public IActionResult AddStockRegister()
        {
            return View();
        }

        [HttpGet("init")]
        public IActionResult GetInitialData(string collegeName)
        {
            return Ok(_service.GetInitialData(collegeName));
        }

        [HttpGet("get")]
        public IActionResult GetByAccession(
            string collegeName,
            string accessionNo)
        {
            var result =
                _service.GetByAccession(collegeName, accessionNo);

            if (result.Count == 0)
            {
                return NotFound("Record not found");
            }

            return Ok(result);
        }

        [HttpGet("book-detail")]
        public IActionResult GetBookDetail(
            string collegeName,
            string title)
        {
            var result =
                _service.GetBookDetail(collegeName, title);

            if (result.Count == 0)
            {
                return NotFound();
            }

            return Ok(result);
        }

        [HttpGet("autocomplete")]
        public IActionResult AutoComplete(
            string collegeName,
            string field,
            string search)
        {
            return Ok(
                _service.AutoComplete(
                    collegeName,
                    field,
                    search
                )
            );
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddBook(
            [FromBody] RequestDto req)
        {
            if (req == null)
            {
                return BadRequest("Invalid request");
            }

            string result =
                await _service.AddBookAsync(req);

            if (result.Contains("already"))
            {
                return BadRequest(result);
            }

            return Ok(result);
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateBook(
            [FromBody] RequestDto req)
        {
            if (req == null)
            {
                return BadRequest("Invalid request");
            }

            string result =
                await _service.UpdateBookAsync(req);

            return Ok(result);
        }
        [HttpGet("colleges")]
        public IActionResult GetColleges()
        {
            var colleges = HttpContext.Session.GetString("Colleges");

            if (string.IsNullOrEmpty(colleges))
                return Ok(new List<string>());

            return Ok(
                JsonSerializer.Deserialize<List<string>>(colleges)
            );
        }
    }
}