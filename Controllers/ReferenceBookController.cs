using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    
    [Route("ReferenceBook")]
    public class ReferenceBookController : Controller
    {
        private readonly IReferenceBookService _referenceBookService;
        public ReferenceBookController(IReferenceBookService referenceBookService)
        {
            _referenceBookService = referenceBookService;
        }
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetReferenceBooks")]
        public async Task<IActionResult>
            GetReferenceBooks(string collegeName)
        {
            if (string.IsNullOrWhiteSpace(collegeName))
                return BadRequest("College Name Required");

            var result =
                await _referenceBookService.GetReferenceBooksAsync(
                    collegeName);

            return Ok(result);
        }
    }
}
