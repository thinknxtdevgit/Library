using lib.DtoModel.SearchISBNDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace lib.Controllers
{
    public class SearchISBNController : Controller
    {
        private readonly ISearchISBNService _service;

        public SearchISBNController(ISearchISBNService service)
        {
            _service = service;
        }

        // ==========================================
        // Serve Razor View
        // ==========================================
        [HttpGet("SearchISBN")]
        public IActionResult Index()
        {
            return View("SearchISBN");
        }

        // ==========================================
        // Search ISBN
        // ==========================================
        [HttpPost("api/SearchISBN/Search")]
        public async Task<IActionResult> Search([FromBody] ISBNSearchRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CollegeName))
            {
                return BadRequest("Select College Name");
            }

            try
            {
                var result = await _service.SearchAsync(request.CollegeName, request.ISBN ?? "");
                return Ok(new
                {
                    success = result.Any(),
                    message = result.Any() ? "Records Found" : "No Records Found",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // Export Excel using ClosedXML
        // ==========================================
        [HttpGet("api/SearchISBN/ExportExcel")]
        public async Task<IActionResult> ExportExcel(string collegeName, string isbn)
        {
            if (string.IsNullOrWhiteSpace(collegeName))
            {
                return BadRequest("Select College Name");
            }

            try
            {
                var fileBytes = await _service.ExportExcelAsync(collegeName, isbn ?? "");
                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "ISBNSearchReport.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
