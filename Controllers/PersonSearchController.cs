using lib.DtoModel.PersonDetailDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    public class PersonSearchController : Controller
    {
        private readonly IPersonSearchService _service;
        public PersonSearchController(IPersonSearchService service)
        {
            _service = service;
        }
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromBody] SearchRequest request)

        {
            var result =
                await _service.SearchPersonAsync(
                    request.IdNo,
                    request.IsUniversityRollNo);

            if (result == null)
            {
                return BadRequest("Invalid ID No.");
            }

            return Ok(result);
        }
    }
}
