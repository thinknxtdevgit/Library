using lib.DtoModel.MasterIssueLimitDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ActionConstraints;

namespace lib.Controllers
{
    [Route("MasterIssueLimit")]
    public class MasterIssueLimitController : Controller
    {
        private readonly IMasterIssueLimitService _service;
        public MasterIssueLimitController(IMasterIssueLimitService service)
        {
            _service = service;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("GetIssueLimit")]
        public async Task<IActionResult> GetIssueLimit(string collegeName,string personType)
        {
            var result = await _service.GetIssueLimitAsync(collegeName, personType);
            return Ok(result);
        }
        [HttpPost("Save")]
        public async Task<IActionResult> Save([FromBody] MasterIssueLimitDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.CollegeName))
                return BadRequest("Select College");

            if (string.IsNullOrWhiteSpace(dto.PersonType))
                return BadRequest("Select Person Type");

            if (dto.IssueLimit <= 0)
                return BadRequest("Enter Issue Limit");

            var check = await _service.GetIssueLimitAsync(dto.CollegeName, dto.PersonType);

            if (check.Success)
            {
                return Ok(await _service.UpdateIssueLimitAsync(dto));
            }

            return Ok(await _service.AddIssueLimitAsync(dto));
        }
    }
}

