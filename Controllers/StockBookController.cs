
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
        [HttpPost]
        public async Task<IActionResult> GetStock([FromBody] CollegeRequestDto request)
        {
            try
            {
                if (request == null || string.IsNullOrEmpty(request.CollegeName))
                    return BadRequest("CollegeName is required");

                var data = await _service.GetStockBooksAsync(request.CollegeName);
                return Ok(data);
            }
            catch (Exception ex)
            {

                return StatusCode(500, ex.ToString());
            }
        }
        [HttpPost]
        public async Task<IActionResult> GetStockPaged([FromBody] PagedRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Search))
                return BadRequest("CollegeName is required");

            var data = await _service.GetStockBooksAsync(request.Search);

            int totalRecords = data.Count;

            var pagedData = data
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return Ok(new PagedResult<StockBookDto>
            {
                Data = pagedData,
                TotalRecords = totalRecords,
                PageNumber = request.PageNumber,
                PageSize = request.PageSize
            });
        }


        // ================= SUMMARY ONLY API =================
        [HttpPost]
        public async Task<IActionResult> GetSummary([FromBody] CollegeRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.CollegeName))
                return BadRequest("CollegeName is required");

            var result = new
            {
                TotalBooks = await _service.GetTotalBooksAsync(request.CollegeName),
                TotalTitles = await _service.GetTotalTitlesAsync(request.CollegeName),
                UnusedBooks = await _service.GetUnusedBooksAsync(request.CollegeName)
            };

            return Ok(result);
        }

        // ================= EXPORT EXCEL =================

        [HttpPost]
        public async Task<IActionResult> ExportExcel([FromBody] CollegeRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.CollegeName))
                return BadRequest("CollegeName is required");

            var fileBytes = await _service.ExportToExcelAsync(request.CollegeName);

            string fileName = $"StockBooks_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

            return File(fileBytes,
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileName);
        }
    }

}

