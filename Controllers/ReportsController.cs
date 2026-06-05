using lib.DtoModel.IssueReportDto;
using lib.DtoModel.StockBookDto;
using lib.Interface;
using lib.Pagination_Helper;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;
        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

    

        [HttpGet("/Reports/IssueReport")]
        public IActionResult IssueReport()
        {
            // Placeholder for Issue Report
            return View("~/Views/StatusIssueRegister/StockBooksDetails.cshtml");
        }

        [HttpPost]
        public async Task<IActionResult> GetStockPaged([FromBody] PagedRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Search))
                return BadRequest("CollegeName is required");

            var data = await _reportService.GetIssueBooksReport(request.Search);

            int totalRecords = data.Count;

            var pagedData = data
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Ok(new PagedResult<IssueBookReportDto>
            {
                Data = pagedData,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            });
        }


        //// ================= COLLEGE LIST =================

        //[HttpPost("college")]
        //public async Task<IActionResult> GetCollegeReport(
        // [FromBody] CollegeRequestDto request)
        //{
        //    try
        //    {
        //        if (request == null ||
        //            string.IsNullOrWhiteSpace(request.CollegeName))
        //        {
        //            return BadRequest("College Name Required");
        //        }

        //        var result =
        //            await _reportService.GetCollegeReportAsync(
        //                request.CollegeName);

        //        return Ok(result);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}
        //// ================= EXPORT EXCEL =================

        //[HttpPost("college/export")]
        //public async Task<IActionResult> ExportCollegeReport(
        //    [FromBody] CollegeRequestDto request)
        //{
        //    try
        //    {
        //        if (request == null ||
        //            string.IsNullOrWhiteSpace(request.CollegeName))
        //        {
        //            return BadRequest("College Name Required");
        //        }

        //        var fileBytes =
        //            await _reportService.ExportCollegeReportAsync(
        //                request.CollegeName);

        //        string fileName =
        //            $"IssueReport_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

        //        return File(
        //            fileBytes,
        //            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        //            fileName);
        //    }
        //    catch (Exception ex)
        //    {
        //        return StatusCode(500, ex.Message);
        //    }
        //}
    }
}
