using lib.DtoModel.FacultySearchDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("FacultySearch")]
    public class FacultySearchController : Controller
    {
        private readonly IFacultySearchService _service;

        public FacultySearchController(IFacultySearchService service)
        {
            _service = service;
        }

        [HttpGet("Index")]
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("SearchFaculty");
        }

        // ==========================
        // Search Faculty
        // ==========================
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] FacultySearchRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CollegeName))
            {
                return BadRequest("Select College Name");
            }

            var result = await _service.SearchFacultyAsync(request);
            return Ok(result);
        }

        // =====================
        // Export Excel
        // =====================
        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] FacultySearchRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CollegeName))
            {
                return BadRequest("Select College Name");
            }

            var file = await _service.ExportFacultyExcelAsync(request);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"FacultySearch_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }
}
