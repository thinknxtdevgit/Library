using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Controllers
{
    [Route("api/report")]
    public class StatusIssueRegisterController : Controller
    {

        private readonly string _connectionString;
        public StatusIssueRegisterController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public class CollegeRequest
        {
            public string CollegeName { get; set; }
        }

        // VIEW PAGE
        [HttpGet("/StatusIssueRegister/StockBooksDetails")]
        public IActionResult StockBooksDetails()
        {
            return View();
        }

        // GET REPORT API
        [HttpPost("college")]
        public async Task<IActionResult> GetCollegeReport([FromBody] CollegeRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.CollegeName))
                    return BadRequest("Invalid College");

                using SqlConnection con = new SqlConnection(_connectionString);

                await con.OpenAsync();

                var issueBookList = new List<Dictionary<string, object>>();

                int totalBooks = 0;
                int issuedBooks = 0;

                string query = @"

    SELECT
        CONVERT(VARCHAR(20), IssueDate, 102) AS IssueDate,
        IDNo,
        WhomIssued,
        Discipline,
        TRY_CAST(AccessionNo AS INT) AS AccessionNo,
        Title,
        Author,
        LastReturnDate,
        Condition,
        Type,
        Category,
        ReceiveDate,
        Remarks
    FROM IssueRegister
    WHERE CollegeName = @CollegeName
    ORDER BY TRY_CAST(AccessionNo AS INT) DESC;

    -- TOTAL BOOKS

    SELECT COUNT(*)
    FROM StockRegister
    WHERE CollegeName = @CollegeName;

    -- TOTAL ISSUED BOOKS

    SELECT COUNT(*)
    FROM IssueRegister
    WHERE CollegeName = @CollegeName;

";
                using SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@CollegeName", req.CollegeName);

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                // ISSUED BOOKS DATA

                while (await dr.ReadAsync())
                {
                    var row = new Dictionary<string, object>();

                    for (int i = 0; i < dr.FieldCount; i++)
                    {
                        row[dr.GetName(i)] =
                            dr.IsDBNull(i) ? null : dr.GetValue(i);
                    }

                    issueBookList.Add(row);
                }

                // TOTAL BOOKS

                if (await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    totalBooks = Convert.ToInt32(dr[0]);
                }

                // TOTAL ISSUED BOOKS

                if (await dr.NextResultAsync() && await dr.ReadAsync())
                {
                    issuedBooks = Convert.ToInt32(dr[0]);
                }

                // TOTAL UNISSUED BOOKS

                int unissuedBooks = totalBooks - issuedBooks;

                return Ok(new
                {
                    totalBooks,
                    issuedBooks,
                    unissuedBooks,
                    issueBooks = issueBookList
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
        // EXPORT EXCEL API
        [HttpPost("college/export")]
        public async Task<IActionResult> ExportCollegeReport([FromBody] CollegeRequest req)
        {
            try
            {
                if (req == null || string.IsNullOrWhiteSpace(req.CollegeName))
                    return BadRequest("Invalid College");

                using SqlConnection con = new SqlConnection(_connectionString);

                await con.OpenAsync();

                string query = @"
                    SELECT 
                        DateEntry,
                        TRY_CAST(AccessionNo AS INT) AS AccessionNo,
                        Title,
                        Author,
                        Edition,
                        Publisher,
                        Year,
                        Pages,
                        Volume,
                        Source,
                        Price,
                        NetPrice,
                        Discount,
                        Type,
                        Category,
                        BillNo,
                        ClassNo,
                        BillDate,
                        BookNo,
                        Remarks,
                        Location,
                        FirstAuthorForeName,
                        FirstAuthorSirName,
                        SecondAuthorForeName,
                        SecondAuthorSirName,
                        ThirdAuthorForeName,
                        ThirdAuthorSirName,
                        MoreThanThreeAuthors,
                        SubTitle,
                        ISBN,
                        Place,
                        Series,
                        BookSize,
                        Subject1,
                        Subject2
                    FROM StockRegister
                    WHERE CollegeName = @CollegeName
                    ORDER BY TRY_CAST(AccessionNo AS INT);
                ";

                using SqlCommand cmd = new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@CollegeName", req.CollegeName);

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();

                DataTable dt = new DataTable();

                dt.Load(dr);

                using XLWorkbook wb = new XLWorkbook();

                var worksheet = wb.Worksheets.Add(dt, "College Report");

                // HEADER STYLE
                var headerRange = worksheet.Range(1, 1, 1, dt.Columns.Count);

                headerRange.Style.Font.Bold = true;

                worksheet.Columns().AdjustToContents();

                using MemoryStream stream = new MemoryStream();

                wb.SaveAs(stream);

                var content = stream.ToArray();

                return File(
                    content,
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    $"{req.CollegeName}_Report.xlsx"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }
        }
    }
}
