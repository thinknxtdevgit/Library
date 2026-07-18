using ClosedXML.Excel;
using DocumentFormat.OpenXml.Bibliography;
using DocumentFormat.OpenXml.Spreadsheet;
using lib.DtoModel.StockBookDto;
using lib.Interface;
using lib.Pagination_Helper;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Service
{
    public class StockBookService: IStockBookService
    {
        private readonly string _connectionString;
 

        public StockBookService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
      
        }

        private static bool IsGlobalStock(string collegeName)
        {
            return string.IsNullOrWhiteSpace(collegeName) ||
                   collegeName.Equals("Global Stock", StringComparison.OrdinalIgnoreCase) ||
                   collegeName.Equals("ALL", StringComparison.OrdinalIgnoreCase);
        }

        // ================= GET STOCK =================
        public async Task<List<StockBookDto>> GetStockBooksAsync(string collegeName)
        {
            var list = new List<StockBookDto>();

            using SqlConnection con = new SqlConnection(_connectionString);
            bool isGlobal = IsGlobalStock(collegeName);

            string sql = @"
SELECT
    DateEntry,
    AccessionNo,
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
    FirstName,
    SirName,
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
FROM StockRegister";

            if (!isGlobal)
            {
                sql += " WHERE LTRIM(RTRIM(CollegeName)) = LTRIM(RTRIM(@CollegeName))";
            }

            using SqlCommand cmd = new SqlCommand(sql, con);
            if (!isGlobal)
            {
                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
            }

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                DateTime? dateEntry = null;

                if (reader["DateEntry"] != DBNull.Value &&
                    reader["DateEntry"] is DateTime dt1)
                {
                    dateEntry = dt1;
                }

                string billDateText = reader["BillDate"]?.ToString();

                DateTime? billDate = null;

                if (!string.IsNullOrWhiteSpace(billDateText))
                {
                    DateTime parsed;

                    if (DateTime.TryParseExact(
                        billDateText,
                        new[] { "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-dd" },
                        System.Globalization.CultureInfo.InvariantCulture,
                        System.Globalization.DateTimeStyles.None,
                        out parsed))
                    {
                        billDate = parsed;
                    }
                }

                list.Add(new StockBookDto
                {
                    DateEntry = dateEntry,

                    AccessionNo = reader["AccessionNo"]?.ToString(),
                    Title = reader["Title"]?.ToString(),
                    Author = reader["Author"]?.ToString(),
                    Edition = reader["Edition"]?.ToString(),
                    Publisher = reader["Publisher"]?.ToString(),

                    Year = reader["Year"]?.ToString(),
                    Pages = reader["Pages"]?.ToString(),
                    Volume = reader["Volume"]?.ToString(),
                    Source = reader["Source"]?.ToString(),

                    Price = reader["Price"]?.ToString(),

                    NetPrice = reader["NetPrice"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(reader["NetPrice"]),

                    Discount = reader["Discount"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(reader["Discount"]),

                    Type = reader["Type"]?.ToString(),
                    Category = reader["Category"]?.ToString(),

                    BillNo = reader["BillNo"]?.ToString(),
                    ClassNo = reader["ClassNo"]?.ToString(),

                    BillDate = billDate?.ToString("yyyy-MM-dd"),

                    BookNo = reader["BookNo"]?.ToString(),
                    Remarks = reader["Remarks"]?.ToString(),
                    Location = reader["Location"]?.ToString(),

                    FirstName = reader["FirstName"]?.ToString(),
                    SirName = reader["SirName"]?.ToString(),

                    FirstAuthorForeName = reader["FirstAuthorForeName"]?.ToString(),
                    FirstAuthorSirName = reader["FirstAuthorSirName"]?.ToString(),
                    SecondAuthorForeName = reader["SecondAuthorForeName"]?.ToString(),
                    SecondAuthorSirName = reader["SecondAuthorSirName"]?.ToString(),
                    ThirdAuthorForeName = reader["ThirdAuthorForeName"]?.ToString(),
                    ThirdAuthorSirName = reader["ThirdAuthorSirName"]?.ToString(),

                    MoreThanThreeAuthors = reader["MoreThanThreeAuthors"]?.ToString(),

                    SubTitle = reader["SubTitle"]?.ToString(),
                    ISBN = reader["ISBN"]?.ToString(),
                    Place = reader["Place"]?.ToString(),
                    Series = reader["Series"]?.ToString(),
                    BookSize = reader["BookSize"]?.ToString(),

                    Subject1 = reader["Subject1"]?.ToString(),
                    Subject2 = reader["Subject2"]?.ToString()
                });
            }
            return list;
        }

        // ================= TOTAL BOOKS =================
        public async Task<int> GetTotalBooksAsync(string collegeName)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            bool isGlobal = IsGlobalStock(collegeName);
            string sql = isGlobal 
                ? "SELECT COUNT(*) FROM StockRegister"
                : "SELECT COUNT(*) FROM StockRegister WHERE CollegeName=@CollegeName";

            using SqlCommand cmd = new SqlCommand(sql, con);
            if (!isGlobal)
            {
                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
            }

            await con.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync();
        }

        // ================= TOTAL TITLES =================
        public async Task<int> GetTotalTitlesAsync(string collegeName)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            bool isGlobal = IsGlobalStock(collegeName);
            string sql = isGlobal
                ? @"SELECT COUNT(*) FROM (
                       SELECT Title, Author 
                       FROM StockRegister 
                       GROUP BY Title, Author) t"
                : @"SELECT COUNT(*) FROM (
                       SELECT Title, Author 
                       FROM StockRegister 
                       WHERE CollegeName=@CollegeName 
                       GROUP BY Title, Author) t";

            using SqlCommand cmd = new SqlCommand(sql, con);
            if (!isGlobal)
            {
                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
            }

            await con.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync();
        }

        // ================= UNUSED BOOKS =================
        public async Task<int> GetUnusedBooksAsync(string collegeName)
        {
            int stock = 0;
            int issued = 0;
            bool isGlobal = IsGlobalStock(collegeName);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                string query1 = isGlobal
                    ? "SELECT COUNT(*) FROM StockRegister"
                    : "SELECT COUNT(*) FROM StockRegister WHERE CollegeName=@CollegeName";
                SqlCommand cmd1 = new SqlCommand(query1, con);
                if (!isGlobal) cmd1.Parameters.AddWithValue("@CollegeName", collegeName);
                stock = (int)await cmd1.ExecuteScalarAsync();

                string query2 = isGlobal
                    ? "SELECT COUNT(*) FROM IssueRegister"
                    : "SELECT COUNT(*) FROM IssueRegister WHERE CollegeName=@CollegeName";
                SqlCommand cmd2 = new SqlCommand(query2, con);
                if (!isGlobal) cmd2.Parameters.AddWithValue("@CollegeName", collegeName);
                issued = (int)await cmd2.ExecuteScalarAsync();
            }

            return stock - issued;
        }

        // ================= EXPORT EXCEL =================
        public async Task<byte[]> ExportToExcelAsync(string collegeName)
        {
            var data = await GetStockBooksAsync(collegeName);

            using var workbook = new XLWorkbook();
            var ws = workbook.Worksheets.Add("StockBooks");

            ws.Cell(1, 1).InsertTable(data);

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
       
    }

}
