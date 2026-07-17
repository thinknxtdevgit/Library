using lib.DtoModel.SearchClassNoDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace lib.Controllers
{
    public class SearchClassNoController : Controller
    {
        private readonly ISearchClassNoService _service;

        public SearchClassNoController(ISearchClassNoService service)
        {
            _service = service;
        }

        // ==========================================
        // Renders Razor View
        // ==========================================
        [HttpGet("SearchClassNo")]
        public IActionResult Index()
        {
            return View("SearchClassNo");
        }

        // ==========================================
        // Search Class No
        // ==========================================
        [HttpPost("api/SearchClassNo/Search")]
        public async Task<IActionResult> Search([FromBody] ClassNoSearchRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CollegeName))
            {
                return BadRequest("Select College Name");
            }

            try
            {
                var result = await _service.SearchAsync(request.CollegeName, request.ClassNo ?? "");
                return Ok(new
                {
                    success = result.Any(),
                    message = result.Any() ? "Records Found" : "_No Record Found_",
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
        [HttpGet("api/SearchClassNo/ExportExcel")]
        public async Task<IActionResult> ExportExcel(string collegeName, string classNo)
        {
            if (string.IsNullOrWhiteSpace(collegeName))
            {
                return BadRequest("Select College Name");
            }

            try
            {
                var fileBytes = await _service.ExportExcelAsync(collegeName, classNo ?? "");
                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "ClassNoReport.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
