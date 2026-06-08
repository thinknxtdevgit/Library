using lib.Interface;
using lib.Service;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("SearchStudentName")]
    public class SearchStudentNameController : Controller
    {
        private readonly ISearchStudentNameService _service;
        public SearchStudentNameController(ISearchStudentNameService service)
        {
            _service = service;
        }
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
        // ==========================
        // Load Colleges
        // ==========================
        [HttpGet("colleges")]
        public async Task<IActionResult> GetColleges()
        {
            var result =
                await _service.GetCollegesAsync();

            return Ok(result);
        }

        // ==========================
        // Search Student
        // ==========================
        [HttpGet("search")]
        public async Task<IActionResult> Search(string collegeName, string studentName)
        {
            if (string.IsNullOrWhiteSpace(collegeName))
            {
                return BadRequest(
                    "Select College Name");
            }

            var result =
                await _service.SearchStudentAsync(
                    collegeName,
                    studentName);

            return Ok(result);
        }
        // =====================
        // Export Excel
        // =====================
        [HttpGet("export")]
        public async Task<IActionResult> Export(
            string collegeName,
            string studentName)
        {
            var file =
                await _service.ExportStudentExcelAsync(
                    collegeName,
                    studentName);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"StudentSearch_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }
}

