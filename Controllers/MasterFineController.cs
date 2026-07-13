using lib.DtoModel.MasterFineDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("MasterFine")]
    public class MasterFineController : Controller
    {
        private readonly IMasterFineService _service;
        public MasterFineController(IMasterFineService service)
        {
            _service=service;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("GetFine")]
        public async Task<IActionResult> GetFine(string collegeName)
        {
            var result = await _service.GetFineAsync(collegeName);

            return Json(result);
        }

        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] MasterFineDto dto)
        {
            var check = await _service.GetFineAsync(dto.CollegeName);

            if (check.Success)
            {
                return Json(await _service.UpdateFineAsync(dto));
            }

            return Json(await _service.AddFineAsync(dto));
        }
    }
}

