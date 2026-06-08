using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("SearchPersonId")]
    public class SearchPersonIdController : Controller
    {
        private readonly ISearchPersonIdService _service;
        public SearchPersonIdController(ISearchPersonIdService service)
        {
            _service = service;
        }
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search(string collegeName, string personId)
        {
            if (string.IsNullOrWhiteSpace(collegeName))
            {
                return BadRequest("Select College Name");
            }

            if (string.IsNullOrWhiteSpace(personId))
            {
                return BadRequest("Enter Person ID");
            }

            var result =
                await _service.SearchAsync(
                    collegeName,
                    personId);

            return Ok(result);
        }
    }
}

