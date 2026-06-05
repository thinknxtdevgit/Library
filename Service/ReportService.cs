using ClosedXML.Excel;
using lib.DtoModel.IssueReportDto;
using lib.Interface;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Service
{
    public class ReportService: BaseService, IReportService
    {

        private readonly string _connectionString;

        public ReportService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection");
        }

        // ========================= REPORT =========================

        public async Task<ReportResponseDto> GetCollegeReportAsync(string collegeName)
        {
            var result = new ReportResponseDto();

            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

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
ORDER BY TRY_CAST(AccessionNo AS INT);

SELECT COUNT(*) FROM StockRegister WHERE CollegeName = @CollegeName;

SELECT COUNT(*) FROM IssueRegister WHERE CollegeName = @CollegeName;
";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            // ISSUED LIST
            while (await dr.ReadAsync())
            {
                result.IssueBooks.Add(new IssueBookReportDto
                {
                    IssueDate = dr["IssueDate"]?.ToString(),
                    IDNo = dr["IDNo"]?.ToString(),
                    WhomIssued = dr["WhomIssued"]?.ToString(),
                    Discipline = dr["Discipline"]?.ToString(),
                    AccessionNo = dr["AccessionNo"]?.ToString(),
                    Title = dr["Title"]?.ToString(),
                    Author = dr["Author"]?.ToString(),
                    LastReturnDate = dr["LastReturnDate"]?.ToString(),
                    Condition = dr["Condition"]?.ToString(),
                    Type = dr["Type"]?.ToString(),
                    Category = dr["Category"]?.ToString(),
                    ReceiveDate = dr["ReceiveDate"]?.ToString(),
                    Remarks = dr["Remarks"]?.ToString()
                });
            }

            // TOTAL BOOKS
            if (await dr.NextResultAsync() && await dr.ReadAsync())
                result.TotalBooks = Convert.ToInt32(dr[0]);

            // ISSUED BOOKS
            if (await dr.NextResultAsync() && await dr.ReadAsync())
                result.IssuedBooks = Convert.ToInt32(dr[0]);

            result.UnissuedBooks = result.TotalBooks - result.IssuedBooks;

            return result;
        }

        // ========================= EXPORT =========================

        public async Task<byte[]> ExportCollegeReportAsync(string collegeName)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            string query = @"SELECT * FROM StockRegister WHERE CollegeName=@CollegeName";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            DataTable dt = new DataTable();
            dt.Load(dr);

            using XLWorkbook wb = new XLWorkbook();
            var ws = wb.Worksheets.Add(dt, "Report");

            ws.Range(1, 1, 1, dt.Columns.Count).Style.Font.Bold = true;
            ws.Columns().AdjustToContents();

            using MemoryStream stream = new MemoryStream();
            wb.SaveAs(stream);

            return stream.ToArray();
        }
        public async Task<List<IssueBookReportDto>> GetIssueBooksReport(string collegeName)
        {
            List<IssueBookReportDto> list = new();

            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

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
        ORDER BY TRY_CAST(AccessionNo AS INT)";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new IssueBookReportDto
                {
                    IssueDate = dr["IssueDate"]?.ToString(),
                    IDNo = dr["IDNo"]?.ToString(),
                    WhomIssued = dr["WhomIssued"]?.ToString(),
                    Discipline = dr["Discipline"]?.ToString(),
                    AccessionNo = dr["AccessionNo"]?.ToString(),
                    Title = dr["Title"]?.ToString(),
                    Author = dr["Author"]?.ToString(),
                    LastReturnDate = dr["LastReturnDate"]?.ToString(),
                    Condition = dr["Condition"]?.ToString(),
                    Type = dr["Type"]?.ToString(),
                    Category = dr["Category"]?.ToString(),
                    ReceiveDate = dr["ReceiveDate"]?.ToString(),
                    Remarks = dr["Remarks"]?.ToString()
                });
            }

            return list;
        }
    }
}
