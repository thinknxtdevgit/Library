using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Controllers
{
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
        public IActionResult StockBooksDetails()
        {
            return View();
        }
        //[HttpPost]
        //[Route("api/report/college")]
        //public IActionResult GetCollegeReport([FromBody] CollegeRequest req)
        //{
        //    string collegeName = req.CollegeName;

        //    if (string.IsNullOrEmpty(collegeName) || collegeName == "Select")
        //        return BadRequest("Invalid College");

        //    var stockTable = GetStockOfBooks(req.CollegeName);

        //    var totalBooks = TotalBooks(collegeName);
        //    var totalTitles = TotalTitles(collegeName);


        //    return Ok(new
        //    {
        //        TotalBooks = totalBooks,
        //        unusedBooks = UnusedBooks(collegeName),
        //        totalTitles = TotalTitles(collegeName),
        //        stockData = ConvertToList(stockTable)
        //    });
        //}

        //private List<Dictionary<string, object>> ConvertToList(DataTable dt)
        //{
        //    var list = new List<Dictionary<string, object>>();

        //    foreach (DataRow row in dt.Rows)
        //    {
        //        var dict = new Dictionary<string, object>();

        //        foreach (DataColumn col in dt.Columns)
        //        {
        //            dict[col.ColumnName] = row[col] == DBNull.Value ? null : row[col];
        //        }

        //        list.Add(dict);
        //    }

        //    return list;
        //}


        //// ================= getStockofBooks =================

        //private DataTable GetStockOfBooks(string collegeName)
        //{
        //    using SqlConnection con = new SqlConnection(_connectionString);

        //    string sql = @"
        //SELECT 
        //    DateEntry,
        //    CAST(AccessionNo AS INT) AS AccessionNo,
        //    Title, Author, Edition, Publisher,
        //    Year, Pages, Volume, Source,
        //    Price, NetPrice, Discount,
        //    Type, Category,
        //    BillNo, ClassNo, BillDate, BookNo,
        //    Remarks, Location,
        //    FirstAuthorForeName, FirstAuthorSirName,
        //    SecondAuthorForeName, SecondAuthorSirName,
        //    ThirdAuthorForeName, ThirdAuthorSirName,
        //    MoreThanThreeAuthors,
        //    SubTitle, ISBN, Place, Series,
        //    BookSize, Subject1, Subject2
        //FROM StockRegister
        //WHERE CollegeName = @CollegeName
        //ORDER BY AccessionNo";

        //    using SqlCommand cmd = new SqlCommand(sql, con);
        //    cmd.Parameters.AddWithValue("@CollegeName", collegeName);

        //    using SqlDataAdapter da = new SqlDataAdapter(cmd);
        //    DataTable dt = new DataTable();
        //        da.Fill(dt);

        //    return dt;
        //}



        //// ================= TotalBooks =================


        //private int TotalBooks(string collegeName)
        //{
        //    int count = 0;

        //    using (SqlConnection con = new SqlConnection(_connectionString))
        //    {
        //        string sql = "select Count(*) from StockRegister where CollegeName=@CollegeName";

        //        SqlCommand cmd = new SqlCommand(sql, con);
        //        cmd.Parameters.AddWithValue("@CollegeName", collegeName);

        //        con.Open();
        //        count = Convert.ToInt32(cmd.ExecuteScalar());
        //    }

        //    return count;
        //}

        //// ================= TotalTitles =================

        //private int TotalTitles(string collegeName)
        //{

        //    int count = 0;

        //    using (SqlConnection con = new SqlConnection(_connectionString))
        //    {
        //        string query = @"
        //    SELECT COUNT(*) 
        //    FROM (
        //        SELECT Title, Author 
        //        FROM StockRegister 
        //        WHERE CollegeName = @CollegeName 
        //        GROUP BY Title, Author
        //    ) AS T";

        //        using (SqlCommand cmd = new SqlCommand(query, con))
        //        {
        //            cmd.Parameters.AddWithValue("@CollegeName", collegeName);
        //            con.Open();

        //            count = Convert.ToInt32(cmd.ExecuteScalar());
        //        }
        //    }

        //    return count;
        //}

        //// ================= UnusedBooks =================

        //private int UnusedBooks(string collegeName)
        //{
        //    int stockCount = 0;
        //    int issuedCount = 0;

        //    using (SqlConnection con = new SqlConnection(_connectionString))
        //    {
        //        con.Open();

        //        // 🔹 Stock count
        //        string sql1 = "SELECT COUNT(*) FROM StockRegister WHERE CollegeName=@CollegeName";
        //        SqlCommand cmd1 = new SqlCommand(sql1, con);
        //        cmd1.Parameters.AddWithValue("@CollegeName", collegeName);

        //        stockCount = Convert.ToInt32(cmd1.ExecuteScalar());

        //        // 🔹 Issued count
        //        string sql2 = "SELECT COUNT(*) FROM IssueRegister WHERE CollegeName=@CollegeName";
        //        SqlCommand cmd2 = new SqlCommand(sql2, con);
        //        cmd2.Parameters.AddWithValue("@CollegeName", collegeName);

        //        issuedCount = Convert.ToInt32(cmd2.ExecuteScalar());
        //    }

        //    return stockCount - issuedCount;
        //}




        [HttpPost]
        [Route("api/report/college")]
        public async Task<IActionResult> GetCollegeReport([FromBody] CollegeRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.CollegeName))
                return BadRequest("Invalid College");

            using SqlConnection con = new SqlConnection(_connectionString);
            await con.OpenAsync();

            var stockList = new List<Dictionary<string, object>>();
            int totalBooks = 0;
            int totalTitles = 0;
            int issuedBooks = 0;

            // MULTIPLE QUERY IN ONE HIT
            string query = @"
        SELECT DateEntry, CAST(AccessionNo AS INT) AS AccessionNo, Title, Author,
               Edition, Publisher, Year, Pages, Volume, Source,
               Price, NetPrice, Discount, Type, Category,
               BillNo, ClassNo, BillDate, BookNo,
               Remarks, Location,
               FirstAuthorForeName, FirstAuthorSirName,
               SecondAuthorForeName, SecondAuthorSirName,
               ThirdAuthorForeName, ThirdAuthorSirName,
               MoreThanThreeAuthors,
               SubTitle, ISBN, Place, Series,
               BookSize, Subject1, Subject2
        FROM StockRegister
        WHERE CollegeName=@CollegeName
        ORDER BY AccessionNo;

        SELECT COUNT(*) FROM StockRegister WHERE CollegeName=@CollegeName;

        SELECT COUNT(*) FROM IssueRegister WHERE CollegeName=@CollegeName;

        SELECT COUNT(*) FROM (
            SELECT Title, Author 
            FROM StockRegister 
            WHERE CollegeName=@CollegeName
            GROUP BY Title, Author
        ) T;
    ";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", req.CollegeName);

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            //  STOCK DATA
            while (await dr.ReadAsync())
            {
                var row = new Dictionary<string, object>();

                for (int i = 0; i < dr.FieldCount; i++)
                {
                    row[dr.GetName(i)] = dr.IsDBNull(i) ? null : dr.GetValue(i);
                }

                stockList.Add(row);
            }

            //  TOTAL BOOKS
            if (await dr.NextResultAsync() && await dr.ReadAsync())
                totalBooks = dr.GetInt32(0);

            //  ISSUED BOOKS
            if (await dr.NextResultAsync() && await dr.ReadAsync())
                issuedBooks = dr.GetInt32(0);

            //  TOTAL TITLES
            if (await dr.NextResultAsync() && await dr.ReadAsync())
                totalTitles = dr.GetInt32(0);

            int unusedBooks = totalBooks - issuedBooks;

            return Ok(new
            {
                totalBooks,
                totalTitles,
                unusedBooks,
                stockData = stockList
            });
        }

    }
}
