
using lib.DtoModel.StockBookDto;
using lib.Interface;
using lib.Pagination_Helper;

//using lib.Pagination_Helper;
using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    public class StockBookController : Controller
    {
        private readonly IStockBookService _service;
        public StockBookController(IStockBookService service)
        {
            _service = service;
        }

        // ================= INDEX (GRID + SUMMARY) =================
        [HttpGet]
        public async Task<IActionResult> Index(string collegeName)
        {
            if (string.IsNullOrEmpty(collegeName))
            {
                ViewBag.Message = "Please select College Name";
                return View(new List<StockBookDto>());
            }

            var data = await _service.GetStockBooksAsync(collegeName);

            ViewBag.TotalBooks = await _service.GetTotalBooksAsync(collegeName);
            ViewBag.TotalTitles = await _service.GetTotalTitlesAsync(collegeName);
            ViewBag.UnusedBooks = await _service.GetUnusedBooksAsync(collegeName);

            ViewBag.CollegeName = collegeName;

            return View(data);
        }

        // ================= API: GET STOCK (AJAX SUPPORT) =================
        //[HttpPost]
        //public async Task<IActionResult> GetStock([FromBody] CollegeRequestDto request)
        //{
        //    try
        //    {
        //        if (request == null || string.IsNullOrEmpty(request.CollegeName))
        //            return BadRequest("CollegeName is required");

        //        var data = await _service.GetStockBooksAsync(request.CollegeName);
        //        return Ok(data);
        //    }
        //    catch (Exception ex)
        //    {

        //        return StatusCode(500, ex.ToString());
        //    }
        //}
        [HttpPost]
        public async Task<IActionResult> GetStockPaged([FromBody] PagedRequest request)
        {
            string collegeName = request?.Search ?? string.Empty;

            var data = await _service.GetStockBooksAsync(collegeName);

            int totalRecords = data.Count;

            int pageNumber = request?.PageNumber ?? 1;
            int pageSize = request?.PageSize ?? 10;
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;

            var pagedData = data
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return Ok(new PagedResult<StockBookDto>
            {
                Data = pagedData,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            });
        }


        // ================= SUMMARY ONLY API =================
        [HttpPost]
        public async Task<IActionResult> GetSummary([FromBody] CollegeRequestDto request)
        {
            string collegeName = request?.CollegeName ?? string.Empty;

            var result = new
            {
                TotalBooks = await _service.GetTotalBooksAsync(collegeName),
                TotalTitles = await _service.GetTotalTitlesAsync(collegeName),
                UnusedBooks = await _service.GetUnusedBooksAsync(collegeName)
            };

            return Ok(result);
        }

        // ================= EXPORT EXCEL =================

        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromBody] CollegeRequestDto request)
        {
            string collegeName = request?.CollegeName ?? string.Empty;

            var fileBytes = await _service.ExportToExcelAsync(collegeName);

            string fileName = string.IsNullOrWhiteSpace(collegeName) || collegeName.Equals("Global Stock", StringComparison.OrdinalIgnoreCase)
                ? $"GlobalStock_{DateTime.Now:yyyyMMddHHmmss}.xlsx"
                : $"StockBooks_{collegeName.Replace(" ", "_")}_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }

}

