using lib.DtoModel.SearchAccessionDto;
using lib.Interface;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace lib.Controllers
{
    public class SearchAccessionController : Controller
    {
        private readonly ISearchAccessionService _service;

        public SearchAccessionController(ISearchAccessionService service)
        {
            _service = service;
        }

        // ==========================================
        // Serve Razor View
        // ==========================================
        [HttpGet("SearchAccession")]
        public IActionResult Index()
        {
            return View("SearchAccession");
        }

        // ==========================================
        // Get Dropdowns data
        // ==========================================
        [HttpGet("api/SearchAccession/Dropdowns")]
        public async Task<IActionResult> GetDropdowns(string collegeName)
        {
            if (string.IsNullOrWhiteSpace(collegeName))
            {
                return BadRequest("Select College Name");
            }

            try
            {
                var publishers = await _service.GetPublishersAsync(collegeName);
                var categories = await _service.GetCategoriesAsync(collegeName);
                var sources = await _service.GetSourcesAsync(collegeName);

                return Ok(new
                {
                    publishers,
                    categories,
                    sources
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // Search Accession details
        // ==========================================
        [HttpGet("api/SearchAccession/Search")]
        public async Task<IActionResult> Search(string collegeName, string accessionNo)
        {
            if (string.IsNullOrWhiteSpace(collegeName))
            {
                return BadRequest("Select College Name");
            }
            if (string.IsNullOrWhiteSpace(accessionNo))
            {
                return BadRequest("Enter Accession No.");
            }

            try
            {
                var result = await _service.SearchAccessionAsync(collegeName, accessionNo);
                if (result == null)
                {
                    return Ok(new
                    {
                        success = false,
                        message = "_No Record Found_"
                    });
                }

                return Ok(new
                {
                    success = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // Stream Student Snap
        // ==========================================
        [HttpGet("api/SearchAccession/StudentImage")]
        public async Task<IActionResult> GetStudentImage(string collegeName, long idNo)
        {
            try
            {
                var imageBytes = await _service.GetStudentImageAsync(collegeName, idNo);
                if (imageBytes != null && imageBytes.Length > 0)
                {
                    return File(imageBytes, "image/jpeg");
                }
                return NotFound();
            }
            catch
            {
                return NotFound();
            }
        }

        // ==========================================
        // Update Stock details
        // ==========================================
        [HttpPost("api/SearchAccession/UpdateStock")]
        public async Task<IActionResult> UpdateStock([FromBody] AccessionUpdateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CollegeName))
            {
                return BadRequest("Select College Name");
            }
            if (string.IsNullOrWhiteSpace(request.AccessionNo))
            {
                return BadRequest("Enter Accession No.");
            }

            // Single quote verification matches the legacy checks
            if (request.Title != null && request.Title.Contains("'"))
            {
                return BadRequest(" ' is not allowed in Title ");
            }
            if (request.FirstAuthorForeName != null && request.FirstAuthorForeName.Contains("'"))
            {
                return BadRequest(" ' is not allowed in Author Forename ");
            }
            if (request.SecondAuthorForeName != null && request.SecondAuthorForeName.Contains("'"))
            {
                return BadRequest(" ' is not allowed in Author Forename ");
            }
            if (request.ThirdAuthorForeName != null && request.ThirdAuthorForeName.Contains("'"))
            {
                return BadRequest(" ' is not allowed in Author Forename ");
            }
            if (request.FirstAuthorSirName != null && request.FirstAuthorSirName.Contains("'"))
            {
                return BadRequest(" ' is not allowed in Author Surname ");
            }
            if (request.SecondAuthorSirName != null && request.SecondAuthorSirName.Contains("'"))
            {
                return BadRequest(" ' is not allowed in Author Surname ");
            }
            if (request.ThirdAuthorSirName != null && request.ThirdAuthorSirName.Contains("'"))
            {
                return BadRequest(" ' is not allowed in Author Surname ");
            }
            if (request.Edition != null && request.Edition.Contains("'"))
            {
                return BadRequest("Invalid Edition");
            }
            if (request.Location != null && request.Location.Contains("'"))
            {
                return BadRequest("Invalid Location");
            }
            if (request.SubTitle != null && request.SubTitle.Contains("'"))
            {
                return BadRequest("Invalid Subtitle");
            }
            if (request.Subject1 != null && request.Subject1.Contains("'"))
            {
                return BadRequest("Invalid Subject1");
            }
            if (request.Subject2 != null && request.Subject2.Contains("'"))
            {
                return BadRequest("Invalid Subject2");
            }
            if (request.Remarks != null && request.Remarks.Contains("'"))
            {
                return BadRequest("Invalid Remarks");
            }
            if (request.Series != null && request.Series.Contains("'"))
            {
                return BadRequest("Invalid Series");
            }

            try
            {
                var success = await _service.UpdateStockAsync(request);
                if (success)
                {
                    return Ok(new { success = true, message = "Updated Successfully" });
                }
                return BadRequest("Failed to update Stock details");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ==========================================
        // Update Issue Remarks
        // ==========================================
        [HttpPost("api/SearchAccession/UpdateIssue")]
        public async Task<IActionResult> UpdateIssue([FromBody] IssueUpdateRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.CollegeName))
            {
                return BadRequest("Select College Name");
            }
            if (string.IsNullOrWhiteSpace(request.AccessionNo))
            {
                return BadRequest("Enter Accession No");
            }
            if (request.IDNo <= 0)
            {
                return BadRequest("Zero ID No. Length is not allowed.");
            }

            try
            {
                var success = await _service.UpdateIssueRemarksAsync(request.CollegeName, request.AccessionNo, request.IDNo, request.Remarks ?? "");
                if (success)
                {
                    return Ok(new { success = true, message = "Issue Detail Update Successfully" });
                }
                return BadRequest("Failed to update Issue details");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
    }
}
