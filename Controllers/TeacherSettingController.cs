using lib.DtoModel.TeacherSettingDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("TeacherSetting")]
    public class TeacherSettingController : Controller
    {
        private readonly ITeacherSettingService _service;

        public TeacherSettingController(ITeacherSettingService service)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet("GetTeachers")]
        public async Task<IActionResult> GetTeachers(string collegeName)
        {
            return Ok(await _service.GetTeachers(collegeName));
        }

        [HttpGet("GetTotalTeachers")]
        public async Task<IActionResult> GetTotalTeachers(string collegeName)
        {
            return Ok(await _service.GetTotalTeachers(collegeName));
        }

        [HttpPost("AddTeacher")]
        public async Task<IActionResult> AddTeacher([FromBody] TeacherSettingDto dto)
        {
            return Ok(await _service.AddTeacher(dto));
        }

        [HttpPut("UpdateTeacher/{oldId}")]
        public async Task<IActionResult> UpdateTeacher(string oldId, [FromBody] TeacherSettingDto dto)
        {
            return Ok(await _service.UpdateTeacher(oldId, dto));
        }

        [HttpGet("ExportExcel")]
        public async Task<IActionResult> ExportExcel(string collegeName)
        {
            var file = await _service.ExportExcelAsync(collegeName);

            return File(
                file,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"TeacherList_{collegeName}.xlsx");
        }
    
    }
}
