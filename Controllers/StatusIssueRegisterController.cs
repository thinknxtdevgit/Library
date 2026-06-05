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
            if (req == null || string.IsNullOrWhiteSpace(req.CollegeName))
                return BadRequest("Invalid College");

            var result = await _reportService.GetCollegeReportAsync(req.CollegeName);
            return Ok(result);
        }

        [HttpPost("college/export")]
        public async Task<IActionResult> ExportCollegeReport([FromBody] CollegeRequestDto req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.CollegeName))
                return BadRequest("Invalid College");

            var file = await _reportService.ExportCollegeReportAsync(req.CollegeName);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"{req.CollegeName}_Report.xlsx"
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







