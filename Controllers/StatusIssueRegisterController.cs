using lib.DtoModel.IssueReportDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("api/report")]
    public class StatusIssueRegisterController : Controller
    {
        private readonly IReportService _reportService;

        public StatusIssueRegisterController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("/StatusIssueRegister/StockBooksDetails")]
        public IActionResult StockBooksDetails()
        {
            return View();
        }

        [HttpPost("college")]
        public async Task<IActionResult> GetCollegeReport([FromBody] CollegeRequestDto req)
        {
            string collegeName = req?.CollegeName ?? string.Empty;
            var result = await _reportService.GetCollegeReportAsync(collegeName);
            return Ok(result);
        }

        [HttpPost("college/export")]
        public async Task<IActionResult> ExportCollegeReport([FromBody] CollegeRequestDto req)
        {
            string collegeName = req?.CollegeName ?? string.Empty;
            var file = await _reportService.ExportCollegeReportAsync(collegeName);

            string fileName = string.IsNullOrWhiteSpace(collegeName) || collegeName.Equals("Global Catalog", StringComparison.OrdinalIgnoreCase)
                ? "GlobalCatalog_Report.xlsx"
                : $"{collegeName.Replace(" ", "_")}_Report.xlsx";

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName
            );
        }
        [HttpGet("colleges")]
        public IActionResult GetColleges()
        {
            try
            {
                var data = HttpContext.Session.GetString("Colleges");

                if (string.IsNullOrEmpty(data))
                    return Ok(new List<string>());

                var list =
                    System.Text.Json.JsonSerializer.Deserialize<List<string>>(data)
                    ?? new List<string>();

                return Ok(list);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = ex.Message
                });
            }
        }
    }
}







