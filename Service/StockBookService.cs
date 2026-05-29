using ClosedXML.Excel;
using lib.DtoModel.StockBookDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class StockBookService: IStockBookService
    {
        private readonly string _connectionString;

        public StockBookService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }

        // ================= GET STOCK =================
        public async Task<List<StockBookDto>> GetStockBooksAsync(string collegeName)
        {
            var list = new List<StockBookDto>();

            using SqlConnection con = new SqlConnection(_connectionString);
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
FROM StockRegister
WHERE LTRIM(RTRIM(CollegeName)) = LTRIM(RTRIM(@CollegeName))";

            using SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            await con.OpenAsync();
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new StockBookDto
                {
                    DateEntry = reader["DateEntry"] == DBNull.Value
    ? (DateTime?)null
    : Convert.ToDateTime(reader["DateEntry"]),

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
                    NetPrice = reader["NetPrice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["NetPrice"]),
                    Discount = reader["Discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["Discount"]),

                    Type = reader["Type"]?.ToString(),
                    Category = reader["Category"]?.ToString(),

                    BillNo = reader["BillNo"]?.ToString(),
                    ClassNo = reader["ClassNo"]?.ToString(),

                    BillDate = reader["BillDate"] == DBNull.Value
    ? null
    : Convert.ToDateTime(reader["BillDate"]).ToString("yyyy-MM-dd"),

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
            string sql = "SELECT COUNT(*) FROM StockRegister WHERE CollegeName=@CollegeName";

            using SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            await con.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync();
        }

        // ================= TOTAL TITLES =================
        public async Task<int> GetTotalTitlesAsync(string collegeName)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            string sql = @"SELECT COUNT(*) FROM (
                       SELECT Title, Author 
                       FROM StockRegister 
                       WHERE CollegeName=@CollegeName 
                       GROUP BY Title, Author) t";

            using SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            await con.OpenAsync();
            return (int)await cmd.ExecuteScalarAsync();
        }

        // ================= UNUSED BOOKS =================
        public async Task<int> GetUnusedBooksAsync(string collegeName)
        {
            int stock = 0;
            int issued = 0;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                SqlCommand cmd1 = new SqlCommand("SELECT COUNT(*) FROM StockRegister WHERE CollegeName=@CollegeName", con);
                cmd1.Parameters.AddWithValue("@CollegeName", collegeName);
                stock = (int)await cmd1.ExecuteScalarAsync();

                SqlCommand cmd2 = new SqlCommand("SELECT COUNT(*) FROM IssueRegister WHERE CollegeName=@CollegeName", con);
                cmd2.Parameters.AddWithValue("@CollegeName", collegeName);
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
