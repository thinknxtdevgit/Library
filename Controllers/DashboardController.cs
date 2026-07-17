using lib.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace lib.Controllers
{
    public class DashboardController : Controller
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("/Dashboard")]
        public async Task<IActionResult> Index()
        {
            // Retrieve authorized colleges list from session
            var data = HttpContext.Session.GetString("Colleges");
            List<string> colleges = new List<string>();
            if (!string.IsNullOrEmpty(data))
            {
                colleges = JsonSerializer.Deserialize<List<string>>(data) ?? new List<string>();
            }

            // Retrieve dynamic stats
            var stats = await _dashboardService.GetDashboardStatsAsync(colleges);

            return View(stats);
        }

        [HttpGet("/api/Dashboard/QuickSearch")]
        public async Task<IActionResult> QuickSearch(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(new List<lib.DtoModel.DashboardDto.QuickSearchResultDto>());
            }

            var data = HttpContext.Session.GetString("Colleges");
            List<string> colleges = new List<string>();
            if (!string.IsNullOrEmpty(data))
            {
                colleges = JsonSerializer.Deserialize<List<string>>(data) ?? new List<string>();
            }

            var results = await _dashboardService.QuickSearchAsync(colleges, query);
            return Ok(results);
        }
    }
}
