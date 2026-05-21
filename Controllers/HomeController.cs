using lib.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace lib.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        public IActionResult Dashboard()
        {
            return View();
        }
    }
}
//    public class CollegeDataResponse
//    {
//        public int AccessionNo { get; set; }
//        public List<string> Publishers { get; set; } = new();
//        public List<string> Sources { get; set; } = new();
//        public List<string> Categories { get; set; } = new();
//        public List<string> Titles { get; set; } = new();
//    }
//    public class StockModel
//    {
//        public string CollegeName { get; set; }
//        public int AccessionNo { get; set; }
//        public string Title { get; set; }
//        public string Author { get; set; }
//        public string Publisher { get; set; }
//        public decimal Price { get; set; }
//    }
//    public IActionResult Index()
//    {
//        return View();
//    }

//    [HttpGet("college-data")]
//    public IActionResult GetCollegeData(string collegeName)
//    {
//        if (string.IsNullOrEmpty(collegeName))
//            return BadRequest("College name is required");

//        var result = new CollegeDataResponse();

//        using (SqlConnection con = new SqlConnection(_connectionString))
//        {
//            con.Open();

//            // ✅ Accession No
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT ISNULL(MAX(AccessionNo),0)+1 FROM StockRegister WHERE CollegeName=@CollegeName", con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                result.AccessionNo = Convert.ToInt32(cmd.ExecuteScalar());
//            }

//            // ✅ Publishers
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT DISTINCT Publisher FROM Publishers WHERE CollegeName=@CollegeName", con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                using var dr = cmd.ExecuteReader();
//                while (dr.Read())
//                    result.Publishers.Add(dr[0].ToString());
//            }

//            // ✅ Sources
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT DISTINCT Source FROM SourceBooks WHERE CollegeName=@CollegeName", con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                using var dr = cmd.ExecuteReader();
//                while (dr.Read())
//                    result.Sources.Add(dr[0].ToString());
//            }

//            // ✅ Categories
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT DISTINCT Category FROM Categories WHERE CollegeName=@CollegeName", con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                using var dr = cmd.ExecuteReader();
//                while (dr.Read())
//                    result.Categories.Add(dr[0].ToString());
//            }

//            // ✅ Titles
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT DISTINCT Title FROM StockRegister WHERE CollegeName=@CollegeName", con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                using var dr = cmd.ExecuteReader();
//                while (dr.Read())
//                    result.Titles.Add(dr[0].ToString());
//            }
//        }

//        return Ok(result);
//    }
//    [HttpGet("get-by-accession")]
//    public IActionResult GetByAccession(string collegeName, int accessionNo)
//    {
//        if (string.IsNullOrEmpty(collegeName) || accessionNo <= 0)
//            return BadRequest("Invalid input");

//        var result = new Dictionary<string, object>();

//        using (SqlConnection con = new SqlConnection(_connectionString))
//        {
//            con.Open();

//            string sql = @"SELECT * FROM StockRegister 
//                   WHERE CollegeName = @CollegeName 
//                   AND AccessionNo = @AccessionNo";

//            using (SqlCommand cmd = new SqlCommand(sql, con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);

//                using (SqlDataReader dr = cmd.ExecuteReader())
//                {
//                    if (dr.Read())
//                    {
//                        for (int i = 0; i < dr.FieldCount; i++)
//                        {
//                            result[dr.GetName(i)] = dr[i] == DBNull.Value ? null : dr[i];
//                        }
//                    }
//                    else
//                    {
//                        return NotFound("Record not found");
//                    }
//                }
//            }
//        }

//        return Ok(result);
//    }
//    [HttpGet("init")]
//    public IActionResult GetInitialData(string collegeName)
//    {
//        if (string.IsNullOrEmpty(collegeName))
//            return BadRequest("College name required");

//        var result = new
//        {
//            AccessionNo = 1,
//            Publishers = new List<string>(),
//            Sources = new List<string>(),
//            Categories = new List<string>(),
//            Titles = new List<string>()
//        };

//        using (SqlConnection con = new SqlConnection(_connectionString))
//        {
//            con.Open();

//            // Accession No
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT ISNULL(MAX(AccessionNo),0)+1 FROM StockRegister WHERE CollegeName=@CollegeName", con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                result = new
//                {
//                    AccessionNo = Convert.ToInt32(cmd.ExecuteScalar()),
//                    Publishers = result.Publishers,
//                    Sources = result.Sources,
//                    Categories = result.Categories,
//                    Titles = result.Titles
//                };
//            }

//            // Publishers
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT DISTINCT Publisher FROM Publishers WHERE CollegeName=@CollegeName", con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                using var dr = cmd.ExecuteReader();
//                while (dr.Read())
//                    result.Publishers.Add(dr[0].ToString());
//            }

//            // Sources
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT DISTINCT Source FROM SourceBooks WHERE CollegeName=@CollegeName", con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                using var dr = cmd.ExecuteReader();
//                while (dr.Read())
//                    result.Sources.Add(dr[0].ToString());
//            }

//            // Categories
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT DISTINCT Category FROM Categories WHERE CollegeName=@CollegeName", con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                using var dr = cmd.ExecuteReader();
//                while (dr.Read())
//                    result.Categories.Add(dr[0].ToString());
//            }

//            // Titles
//            using (SqlCommand cmd = new SqlCommand(
//                "SELECT DISTINCT Title FROM StockRegister WHERE CollegeName=@CollegeName", con))
//            {
//                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//                using var dr = cmd.ExecuteReader();
//                while (dr.Read())
//                    result.Titles.Add(dr[0].ToString());
//            }
//        }

//        return Ok(result);
//    }
//    [HttpGet("book-detail")]
//    public IActionResult GetBookDetail(string collegeName, string title)
//    {
//        using SqlConnection con = new SqlConnection(_connectionString);
//        con.Open();

//        string sql = @"SELECT TOP 1 * FROM StockRegister 
//               WHERE CollegeName=@CollegeName AND Title=@Title";

//        using SqlCommand cmd = new SqlCommand(sql, con);
//        cmd.Parameters.AddWithValue("@CollegeName", collegeName);
//        cmd.Parameters.AddWithValue("@Title", title);

//        using var dr = cmd.ExecuteReader();

//        if (!dr.Read())
//            return NotFound();

//        var data = new
//        {
//            Author = dr["Author"].ToString(),
//            Publisher = dr["Publisher"].ToString(),
//            Source = dr["Source"].ToString(),
//            Edition = dr["Edition"].ToString(),
//            Price = dr["Price"].ToString(),
//            Category = dr["Category"].ToString()
//        };

//        return Ok(data);
//    }
//    [HttpPost("add")]
//    public IActionResult AddBook([FromBody] StockModel model)
//    {
//        using SqlConnection con = new SqlConnection(_connectionString);
//        con.Open();

//        string sql = @"INSERT INTO StockRegister
//(CollegeName, AccessionNo, Title, Author, Publisher, Price)
//VALUES (@CollegeName, @AccessionNo, @Title, @Author, @Publisher, @Price)";

//        using SqlCommand cmd = new SqlCommand(sql, con);

//        cmd.Parameters.AddWithValue("@CollegeName", model.CollegeName);
//        cmd.Parameters.AddWithValue("@AccessionNo", model.AccessionNo);
//        cmd.Parameters.AddWithValue("@Title", model.Title);
//        cmd.Parameters.AddWithValue("@Author", model.Author ?? "None");
//        cmd.Parameters.AddWithValue("@Publisher", model.Publisher);
//        cmd.Parameters.AddWithValue("@Price", model.Price);

//        cmd.ExecuteNonQuery();

//        return Ok("Added Successfully");
//    }
//    [HttpPut("update")]
//    public IActionResult UpdateBook([FromBody] StockModel model)
//    {
//        using SqlConnection con = new SqlConnection(_connectionString);
//        con.Open();

//        string sql = @"UPDATE StockRegister 
//               SET Title=@Title, Author=@Author, Publisher=@Publisher
//               WHERE CollegeName=@CollegeName AND AccessionNo=@AccessionNo";

//        using SqlCommand cmd = new SqlCommand(sql, con);

//        cmd.Parameters.AddWithValue("@CollegeName", model.CollegeName);
//        cmd.Parameters.AddWithValue("@AccessionNo", model.AccessionNo);
//        cmd.Parameters.AddWithValue("@Title", model.Title);
//        cmd.Parameters.AddWithValue("@Author", model.Author);
//        cmd.Parameters.AddWithValue("@Publisher", model.Publisher);

//        cmd.ExecuteNonQuery();

//        return Ok("Updated Successfully");
//    }
