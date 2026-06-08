using lib.DtoModel.SearchDetailedAccessionDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    [Route("SearchDetailedAccession")]
    public class SearchDetailedAccessionController : Controller
    {
        private readonly ISearchDetailedAccessionService _service;
        public SearchDetailedAccessionController(ISearchDetailedAccessionService service)
        {
            _service = service;
        }
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("search")]
        public async Task<IActionResult> Search(
          string collegeName,
          string accessionNo)
        {
            if (string.IsNullOrWhiteSpace(collegeName))
            {
                return BadRequest(
                    new
                    {
                        message = "Select College Name"
                    });
            }

            if (string.IsNullOrWhiteSpace(accessionNo))
            {
                return BadRequest(
                    new
                    {
                        message = "Enter Accession No."
                    });
            }

            var request =
                new SearchDetailedAccessionRequestDto
                {
                    CollegeName = collegeName,
                    AccessionNo = accessionNo
                };

            var result =
                await _service.SearchAccessionNoAsync(request);

            return Ok(result);
        }
        [HttpGet("export")]
        public async Task<IActionResult> Export(string collegeName, string accessionNo)
        {
            if (string.IsNullOrWhiteSpace(collegeName))
                return BadRequest("Select College Name");

            if (string.IsNullOrWhiteSpace(accessionNo))
                return BadRequest("Enter Accession No.");

            var request =
                new SearchDetailedAccessionRequestDto
                {
                    CollegeName = collegeName,
                    AccessionNo = accessionNo
                };

            var fileBytes =
                await _service.ExportAccessionHistoryAsync(request);

            if (fileBytes == null)
                return BadRequest(
                    "No transactions found against this Accession No.");

            return File(
                fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"AccessionHistory_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        }
    }
}
