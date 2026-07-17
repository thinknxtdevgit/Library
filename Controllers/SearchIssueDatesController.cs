using lib.DtoModel.SearchIssueDatesDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace lib.Controllers
{
    [Route("SearchIssueDates")]
    public class SearchIssueDatesController : Controller
    {
        private readonly ISearchIssueDatesService _service;

        public SearchIssueDatesController(ISearchIssueDatesService service)
        {
            _service = service;
        }

        [HttpGet("Index")]
        [HttpGet("")]
        public IActionResult Index()
        {
            return View("SearchIssueDates");
        }

        // ==========================
        // Search Issue Dates
        // ==========================
        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] IssueDatesSearchRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CollegeName))
            {
                return BadRequest("Select College Name");
            }
            if (request.IssueDateFrom == null || request.IssueDateTo == null)
            {
                return BadRequest("Please enter both start and end dates.");
            }

            var result = await _service.SearchIssueDatesAsync(request);
            return Ok(result);
        }

        // =====================
        // Export Excel
        // =====================
        [HttpGet("export")]
        public async Task<IActionResult> Export([FromQuery] IssueDatesSearchRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CollegeName))
            {
                return BadRequest("Select College Name");
            }
            if (request.IssueDateFrom == null || request.IssueDateTo == null)
            {
                return BadRequest("Please enter both start and end dates.");
            }

            var file = await _service.ExportIssueDatesExcelAsync(request);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"SearchIssueDates_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }
}
