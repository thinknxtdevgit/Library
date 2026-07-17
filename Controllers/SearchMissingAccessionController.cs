using lib.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace lib.Controllers
{
    public class SearchMissingAccessionController : Controller
    {
        private readonly IMissingAccessionService _service;

        public SearchMissingAccessionController(IMissingAccessionService service)
        {
            _service = service;
        }

        // ==========================================
        // Serve Razor View
        // ==========================================
        [HttpGet("SearchMissingAccession")]
        public IActionResult Index()
        {
            return View("SearchMissingAccession");
        }

        // ==========================================
        // Generate and Find Missing Accession Nos
        // ==========================================
        [HttpPost("api/SearchMissingAccession/Search")]
        public async Task<IActionResult> Search(string collegeName)
        {
            if (string.IsNullOrWhiteSpace(collegeName) || collegeName == "Select")
            {
                return BadRequest("Select College Name");
            }

            try
            {
                var result = await _service.GenerateAndFindMissingAsync(collegeName);
                return Ok(new
                {
                    success = result.Any(),
                    message = result.Any() ? "Records Found" : "No Missing Accession Numbers Found",
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
        [HttpGet("api/SearchMissingAccession/ExportExcel")]
        public async Task<IActionResult> ExportExcel(string collegeName)
        {
            if (string.IsNullOrWhiteSpace(collegeName) || collegeName == "Select")
            {
                return BadRequest("Select College Name");
            }

            try
            {
                var fileBytes = await _service.ExportExcelAsync(collegeName);
                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "MissingAccessionNumbers.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
