using lib.DtoModel.SearchBookNoDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace lib.Controllers
{
    public class SearchBookNoController : Controller
    {
        private readonly ISearchBookNoService _service;

        public SearchBookNoController(ISearchBookNoService service)
        {
            _service = service;
        }

        // ==========================================
        // Renders Razor View
        // ==========================================
        [HttpGet("SearchBookNo")]
        public IActionResult Index()
        {
            return View("SearchBookNo");
        }

        // ==========================================
        // Get Authorized Colleges
        // ==========================================
        [HttpGet("api/SearchBookNo/GetColleges")]
        public async Task<IActionResult> GetColleges()
        {
            try
            {
                var colleges = await _service.GetAuthorizedCollegesAsync();
                return Ok(colleges);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // Search Book No
        // ==========================================
        [HttpPost("api/SearchBookNo/Search")]
        public async Task<IActionResult> Search([FromBody] BookNoSearchRequestDto request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CollegeName))
            {
                return BadRequest("Select College Name");
            }

            try
            {
                var result = await _service.SearchAsync(request.CollegeName, request.BookNo ?? "");
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
        [HttpGet("api/SearchBookNo/ExportExcel")]
        public async Task<IActionResult> ExportExcel(string collegeName, string bookNo)
        {
            if (string.IsNullOrWhiteSpace(collegeName))
            {
                return BadRequest("Select College Name");
            }

            try
            {
                var fileBytes = await _service.ExportExcelAsync(collegeName, bookNo ?? "");
                return File(
                    fileBytes,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    "BookNoReport.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
