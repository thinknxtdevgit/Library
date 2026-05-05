using lib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace lib.Controllers
{
    public class AddStockRegisterController : Controller
    {
        private readonly string _connectionString;
        public AddStockRegisterController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        #region 🔹 Common DB Methods

        private object ExecuteScalar(string query, params SqlParameter[] parameters)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddRange(parameters);
            con.Open();
            return cmd.ExecuteScalar();
        }

        private async Task ExecuteNonQueryAsync(string query, List<SqlParameter> parameters)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, con);

            cmd.Parameters.AddRange(parameters.ToArray());

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
        }

        private List<string> ExecuteList(string query, params SqlParameter[] parameters)
        {
            List<string> list = new();

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddRange(parameters);

            con.Open();
            using var dr = cmd.ExecuteReader();

            while (dr.Read())
                list.Add(dr[0].ToString());

            return list;
        }

        private Dictionary<string, object> ExecuteSingleRow(string query, params SqlParameter[] parameters)
        {
            var result = new Dictionary<string, object>();

            using SqlConnection con = new SqlConnection(_connectionString);
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddRange(parameters);

            con.Open();
            using var dr = cmd.ExecuteReader();

            if (dr.Read())
            {
                for (int i = 0; i < dr.FieldCount; i++)
                    result[dr.GetName(i)] = dr[i] == DBNull.Value ? null : dr[i];
            }

            return result;
        }

        #endregion

        #region 🔹 INIT API (Dropdown + Accession)

        [HttpGet("/init")]
        public IActionResult GetInitialData(string collegeName)
        {
            if (string.IsNullOrEmpty(collegeName))
                return BadRequest("College required");

            return Ok(new
            {
                AccessionNo = Convert.ToInt32(
                    ExecuteScalar(
                        "SELECT ISNULL(MAX(AccessionNo),0)+1 FROM StockRegister WHERE CollegeName=@CollegeName",
                        new SqlParameter("@CollegeName", collegeName)
                    )
                ),

                Publishers = ExecuteList(
                    "SELECT DISTINCT Publisher FROM Publishers WHERE CollegeName=@CollegeName",
                    new SqlParameter("@CollegeName", collegeName)
                ),

                Sources = ExecuteList(
                    "SELECT DISTINCT Source FROM SourceBooks WHERE CollegeName=@CollegeName",
                    new SqlParameter("@CollegeName", collegeName)
                ),

                Categories = ExecuteList(
                    "SELECT DISTINCT Category FROM Categories WHERE CollegeName=@CollegeName",
                    new SqlParameter("@CollegeName", collegeName)
                ),

                Titles = ExecuteList(
                    "SELECT DISTINCT Title FROM StockRegister WHERE CollegeName=@CollegeName",
                    new SqlParameter("@CollegeName", collegeName)
                )
            });
        }

        #endregion

        #region 🔹 GET BY ACCESSION

        [HttpGet("get")]
        public IActionResult GetByAccession(string collegeName, int accessionNo)
        {
            var result = ExecuteSingleRow(
                "SELECT * FROM StockRegister WHERE CollegeName=@CollegeName AND AccessionNo=@AccessionNo",
                new SqlParameter("@CollegeName", collegeName),
                new SqlParameter("@AccessionNo", accessionNo)
            );

            return result.Count == 0 ? NotFound("Record not found") : Ok(result);
        }

        #endregion

        #region 🔹 BOOK DETAIL

        [HttpGet("book-detail")]
        public IActionResult GetBookDetail(string collegeName, string title)
        {
            var result = ExecuteSingleRow(
                @"SELECT TOP 1 Author,Publisher,Source,Edition,Price,Category 
                  FROM StockRegister 
                  WHERE CollegeName=@CollegeName AND Title=@Title",
                new SqlParameter("@CollegeName", collegeName),
                new SqlParameter("@Title", title)
            );

            return result.Count == 0 ? NotFound() : Ok(result);
        }

        #endregion

        #region 🔹 AUTOCOMPLETE

        [HttpGet("autocomplete")]
        public IActionResult AutoComplete(string collegeName, string field, string search)
        {
            string column = field switch
            {
                "first" => "FirstAuthorForeName",
                "second" => "SecondAuthorForeName",
                "third" => "ThirdAuthorForeName",
                "surname1" => "FirstAuthorSirName",
                "surname2" => "SecondAuthorSirName",
                "surname3" => "ThirdAuthorSirName",
                "place" => "Place",
                _ => "Title"
            };

            var list = ExecuteList(
                $@"SELECT DISTINCT {column} FROM StockRegister 
                   WHERE CollegeName=@CollegeName AND {column} LIKE @Search",
                new SqlParameter("@CollegeName", collegeName),
                new SqlParameter("@Search", "%" + (search ?? "") + "%")
            );

            return Ok(list);
        }

        private List<SqlParameter> GetBookParameters(RequestModel req)
        {
            return new List<SqlParameter>
    {
        new SqlParameter("@CollegeName", req.CollegeName),
        new SqlParameter("@DateEntry", req.DateEntry ?? DateTime.Now),
        new SqlParameter("@AccessionNo", req.AccessionNo ?? 0),

        new SqlParameter("@Author", string.IsNullOrWhiteSpace(req.Author) ? "None" : req.Author),
        new SqlParameter("@Title", req.Title ?? (object)DBNull.Value),
        new SqlParameter("@Edition", req.Edition ?? (object)DBNull.Value),
        new SqlParameter("@Publisher", req.Publisher ?? (object)DBNull.Value),
        new SqlParameter("@Source", string.IsNullOrWhiteSpace(req.Source) ? (object)DBNull.Value : req.Source),

        new SqlParameter("@Year", req.Year ?? (object)DBNull.Value),
        new SqlParameter("@Pages", req.Pages ?? (object)DBNull.Value),
        new SqlParameter("@Volume", (object)DBNull.Value),

        new SqlParameter("@Price", req.Price ?? 0),
        new SqlParameter("@Discount", req.Discount ?? (object)DBNull.Value),
        new SqlParameter("@NetPrice", req.NetPrice ?? 0),

        new SqlParameter("@Type", req.Type ?? (object)DBNull.Value),
        new SqlParameter("@Category", string.IsNullOrWhiteSpace(req.Category) ? "None" : req.Category),

        new SqlParameter("@BillNo", req.BillNo ?? (object)DBNull.Value),
        new SqlParameter("@BillDate", string.IsNullOrWhiteSpace(req.BillDate) ? (object)DBNull.Value : req.BillDate),

        new SqlParameter("@ClassNo", req.ClassNo ?? (object)DBNull.Value),
        new SqlParameter("@BookNo", req.BookNo ?? (object)DBNull.Value),

        new SqlParameter("@Remarks", req.Remarks ?? (object)DBNull.Value),
        new SqlParameter("@Location", req.Location ?? (object)DBNull.Value),

        new SqlParameter("@FirstAuthorForeName", req.FirstAuthorForename ?? ""),
        new SqlParameter("@FirstAuthorSirName", req.FirstAuthorSirName ?? (object)DBNull.Value),

        new SqlParameter("@SecondAuthorForeName", req.SecondAuthorForename ?? (object)DBNull.Value),
        new SqlParameter("@SecondAuthorSirName", req.SecondAuthorSurname ?? (object)DBNull.Value),

        new SqlParameter("@ThirdAuthorForeName", req.ThirdAuthorForename ?? (object)DBNull.Value),
        new SqlParameter("@ThirdAuthorSirName", req.ThirdAuthorSurname ?? (object)DBNull.Value),

        new SqlParameter("@MoreThanThreeAuthors", req.MoreThanThreeAuthors ?? "False"),

        new SqlParameter("@SubTitle", req.Subtitle ?? (object)DBNull.Value),
        new SqlParameter("@ISBN", req.ISBN ?? (object)DBNull.Value),
        new SqlParameter("@Place", req.Place ?? (object)DBNull.Value),
        new SqlParameter("@Series", req.Series ?? (object)DBNull.Value),

        new SqlParameter("@BookSize", req.Size ?? (object)DBNull.Value),

        new SqlParameter("@Subject1", req.Subject1 ?? (object)DBNull.Value),
        new SqlParameter("@Subject2", req.Subject2 ?? (object)DBNull.Value),

        new SqlParameter("@BindingBook", "Normal"),
        new SqlParameter("@Attachment", (object)DBNull.Value)
    };
        }

        #endregion

        #region 🔹 ADD BOOK

        [HttpPost("add")]
        public async Task<IActionResult> AddBook([FromBody] RequestModel req)
        {
            if (req == null)
                return BadRequest("Invalid request");

            // Duplicate check
            var exists = ExecuteScalar(
                "SELECT COUNT(1) FROM StockRegister WHERE CollegeName=@CollegeName AND AccessionNo=@AccessionNo",
                new SqlParameter("@CollegeName", req.CollegeName),
                new SqlParameter("@AccessionNo", req.AccessionNo)
            );

            if (Convert.ToInt32(exists) > 0)
                return BadRequest("Accession No already exists");

            var query = @"INSERT INTO StockRegister
            (CollegeName, DateEntry, AccessionNo, Author, Title, Edition, Publisher,
            Year, Pages, Source, Price, Discount, NetPrice, Type, Category,
            BillNo, BillDate, ClassNo, BookNo, Remarks, Location,
            FirstAuthorForeName, FirstAuthorSirName,
            SecondAuthorForeName, SecondAuthorSirName,
            ThirdAuthorForeName, ThirdAuthorSirName,
            MoreThanThreeAuthors, SubTitle, ISBN, Place, Series,
            BookSize, Subject1, Subject2, BindingBook, Attachment, Volume)
            VALUES
            (@CollegeName, @DateEntry, @AccessionNo, @Author, @Title, @Edition, @Publisher,
            @Year, @Pages, @Source, @Price, @Discount, @NetPrice, @Type, @Category,
            @BillNo, @BillDate, @ClassNo, @BookNo, @Remarks, @Location,
            @FirstAuthorForeName, @FirstAuthorSirName,
            @SecondAuthorForeName, @SecondAuthorSirName,
            @ThirdAuthorForeName, @ThirdAuthorSirName,
            @MoreThanThreeAuthors, @SubTitle, @ISBN, @Place, @Series,
            @BookSize, @Subject1, @Subject2, @BindingBook, @Attachment, @Volume)";

            await ExecuteNonQueryAsync(query, GetBookParameters(req));

            return Ok("Added Successfully");
        }

        #endregion

        #region 🔹 UPDATE BOOK

        [HttpPut("update")]
        public async Task<IActionResult> UpdateBook([FromBody] RequestModel req)
        {
            var query = @"UPDATE StockRegister SET
            Author=@Author, Title=@Title, Edition=@Edition, Publisher=@Publisher,
            Year=@Year, Pages=@Pages, Source=@Source, Price=@Price, Discount=@Discount,
            NetPrice=@NetPrice, Type=@Type, Category=@Category,
            BillNo=@BillNo, BillDate=@BillDate, ClassNo=@ClassNo,
            BookNo=@BookNo, Remarks=@Remarks, Location=@Location,
            FirstAuthorForeName=@FirstAuthorForeName, FirstAuthorSirName=@FirstAuthorSirName,
            SecondAuthorForeName=@SecondAuthorForeName, SecondAuthorSirName=@SecondAuthorSirName,
            ThirdAuthorForeName=@ThirdAuthorForeName, ThirdAuthorSirName=@ThirdAuthorSirName,
            MoreThanThreeAuthors=@MoreThanThreeAuthors,
            SubTitle=@SubTitle, ISBN=@ISBN, Place=@Place, Series=@Series,
            BookSize=@BookSize, Subject1=@Subject1, Subject2=@Subject2
            WHERE CollegeName=@CollegeName AND AccessionNo=@AccessionNo";

            await ExecuteNonQueryAsync(query, GetBookParameters(req));

            return Ok("Updated Successfully");
        }

        #endregion
    }


}




