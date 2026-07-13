using lib.DtoModel.StudentReportDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("StudentReport")]
    public class StudentReportController : Controller
    {
        private readonly IStudentReportService _service;
        public StudentReportController(IStudentReportService studentReportService)
        {
            _service = studentReportService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("Colleges")]
        public async Task<IActionResult> Colleges()
        {
            return Json(await _service.GetCollegesAsync());
        }

        [HttpGet("Courses")]
        public async Task<IActionResult> Courses(string college)
        {
            return Json(await _service.GetCoursesAsync(college));
        }

        [HttpGet("Batch")]
        public async Task<IActionResult> Batch(string college, string course)
        {
            return Json(await _service.GetBatchAsync(college, course));
        }

        [HttpPost("Search")]
        public async Task<IActionResult> Search([FromBody]StudentReportRequestDto request)
        {
            var result = await _service.SearchAsync(request);

            return Json(result);
        }
    }
}

