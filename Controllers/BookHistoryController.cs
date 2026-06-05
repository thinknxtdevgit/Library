using lib.DtoModel.BookHistoryDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("BookHistory")]
    public class BookHistoryController : Controller
    {
        private readonly IBookHistoryService _service;

        public BookHistoryController(IBookHistoryService service)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("search")]
        public async Task<IActionResult> GetBookHistory([FromQuery] BookHistoryRequestDto request)
        {
            if (string.IsNullOrEmpty(request.CollegeName))
                return BadRequest("Select College Name");

            if (string.IsNullOrEmpty(request.AccessionNo))
                return BadRequest("Enter Accession No");

            var data = await _service.GetBookHistoryAsync(
                request.CollegeName,
                request.AccessionNo
            );

            if (data == null || data.Count == 0)
                return NotFound("No Record Found");

            return Ok(data);
        }
      
    }
}
