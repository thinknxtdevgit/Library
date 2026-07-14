using lib.DtoModel.StudentSettingDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("StudentSetting")]
    public class StudentSettingController : Controller
    {
        private readonly IStudentSettingService _service;

        public IActionResult Index()
        {
            return View();
        }
        public StudentSettingController(IStudentSettingService service)
        {
            _service = service;
        }

        [HttpGet("GetStudents")]
        public async Task<IActionResult> GetStudents(string collegeName)
        {
            var data = await _service.GetStudents(collegeName);

            return Ok(data);
        }

        [HttpPost("AddStudent")]
        public async Task<IActionResult> AddStudent(StudentSettingDto dto)
        {
            bool result = await _service.AddStudent(dto);

            return Ok(result);
        }

        [HttpPut("UpdateStudent/{oldId}")]
        public async Task<IActionResult> UpdateStudent(int oldId, StudentSettingDto dto)
        {
            bool result = await _service.UpdateStudent(oldId, dto);

            return Ok(result);
        }
        [HttpGet("ExportExcel")]
        public async Task<IActionResult> ExportExcel(string collegeName)
        {
            var file = await _service.ExportExcelAsync(collegeName);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"StudentList_{collegeName}.xlsx");
        }

    }
}
