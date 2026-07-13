using lib.DtoModel.StaffReportDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("StaffReport")]
    public class StaffReportController : Controller
    {
        private readonly IStaffReportService _service;

        public StaffReportController(IStaffReportService service)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpPost("Search")]
        public async Task<IActionResult> Search(
            [FromBody] StaffReportRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.CollegeName))
            {
                return BadRequest("Select College Name");
            }

            var result = await _service.SearchAsync(request);

            if (result.StaffList.Count == 0)
            {
                return NotFound(new
                {
                    Message = "No record found for Print"
                });
            }

            return Ok(result);
        }
    }
}

