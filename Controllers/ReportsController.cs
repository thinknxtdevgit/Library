using Microsoft.AspNetCore.Mvc;

namespace lib.Controllers
{
    public class ReportsController : Controller
    {
        [HttpGet("/Reports")]
        public IActionResult Index()
        {
            return View("~/Views/StatusIssueRegister/StockBooksDetails.cshtml");
        }

        [HttpGet("/Reports/IssueReport")]
        public IActionResult IssueReport()
        {
            // Placeholder for Issue Report
            return View("~/Views/StatusIssueRegister/StockBooksDetails.cshtml");
        }
    }
}
