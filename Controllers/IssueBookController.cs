using lib.DtoModel.IssueBook;
using lib.Interface;
using lib.Models;
using lib.Models.IssueBook;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Controllers
{
    public class IssueBookController : Controller
    {
        private readonly IIssueBookService _issueBookService;

        public IssueBookController(
            IIssueBookService issueBookService)
        {
            _issueBookService = issueBookService;
        }

        // =====================================================
        // VIEW
        // =====================================================

        [HttpGet("/IssueBook")]
        public IActionResult Create()
        {
            return View("IssueBook");
        }

        [HttpGet("/IssueBook/List")]
        public IActionResult Index()
        {
            return RedirectToAction(nameof(Create));
        }

        // =====================================================
        // CHECK USER + BOOK DETAIL
        // =====================================================

        [HttpPost("api/IssueBook/checkid")]
        public async Task<IActionResult> CheckId(
            [FromBody] IssueBookRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new IssueBookResponseDto
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            request.CollegeName =
                HttpContext.Session.GetString(
                    "CollegeName");

            var result =
                await _issueBookService
                .CheckIdAsync(request);

            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }

        // =====================================================
        // ISSUE BOOK
        // =====================================================

        [HttpPost("api/IssueBook/issue")]
        public async Task<IActionResult> IssueBook(
            [FromBody] IssueBookRequestDto request)
        {
            if (request == null)
            {
                return BadRequest(new IssueBookResponseDto
                {
                    Success = false,
                    Message = "Invalid request"
                });
            }

            request.CollegeName =
                HttpContext.Session.GetString(
                    "CollegeName");

            var result =
                await _issueBookService
                .IssueBookAsync(request);

            return result.Success
                ? Ok(result)
                : BadRequest(result);
        }
        [HttpGet("check-accession")]
        public async Task<IActionResult> CheckAccession([FromQuery] string accessionNo, [FromQuery] string collegeName)
        {
            var result =
                await _issueBookService.CheckAccessionDetailAsync(
                    accessionNo,
                    collegeName);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
    }


}