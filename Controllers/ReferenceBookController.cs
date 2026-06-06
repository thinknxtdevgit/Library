using lib.Interface;
using lib.Pagination_Helper;
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
        [HttpPost("GetReferenceBooksPaged")]
        public async Task<IActionResult> GetReferenceBooksPaged([FromBody] PagedRequest request)
        {
            try
            {
                var result =
                    await _referenceBookService
                    .GetReferenceBooksAsyncPages(
                        request.Search,
                        request.PageNumber,
                        request.PageSize);

                return Ok(result);
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }
    }
}
