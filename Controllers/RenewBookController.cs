using lib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Controllers
{
    public class RenewBookController : Controller
    {
        private readonly string _connectionString;
        public RenewBookController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpPost]
        [Route("api/book/renew")]
        public IActionResult RenewBook([FromBody] RenewBookRequest req)
        {
            if (req.AccessionNo == 0)
                return BadRequest("Enter Accession No.");

            var info = GetBookInfo(req.AccessionNo);
             
            
            //var extra = GetStudentStaffDetail(info.IDNo);
            var Snap = GetPhoto(info.IDNo);

            if (info == null)
                return BadRequest("Accession No. does not exist");

            // =========================
            // ONLY VIEW
            // =========================
            if (string.IsNullOrWhiteSpace(req.Signature))
            {
                return Ok(new
                {
                    success = true,
                    mode = "view",
                    data = info,
                    //Details=extra,
                    snap=Snap
                });
            }
            // =========================
            //  RENEW
            // =========================
            string userNameFromLogin = GetUserFromSignature(req.Signature);

            if (string.IsNullOrEmpty(info.Name))
                return BadRequest("Zero 'Name' Length is not allowed");

            if (string.IsNullOrEmpty(info.IDNo))
                return BadRequest("Zero 'ID No.' Length is not allowed");

            if (string.IsNullOrEmpty(info.Title))
                return BadRequest("Zero 'Title' Length is not allowed");

            if (req.Signature.Contains("'"))
                return BadRequest("Signature does't Match");

            // Date check
            if (Convert.ToDateTime(info.LastReturnDate) < DateTime.Now)
                return BadRequest("Can't Renew, Last Return Date is Over");

            if (!CheckSignature(req.Signature))
                return BadRequest("Invalid Signature");

            //  Renew
            Renew(req.AccessionNo);

            //Transaction
            AddTransaction(info, req.Signature);

            return Ok(new
            {
                success = true,
                mode = "renew",
                message = "Renewed Successfully"
            });
        }

        private dynamic GetBookInfo(long accessionNo)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT WhomIssued,IDNo,Title,IssueDate,LastReturnDate,IssueDate,
                       CollegeName,Type,AccessionNo,Author,Discipline
                       FROM IssueRegister
                       WHERE AccessionNo=@AccessionNo";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);


                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    return new
                    {
                        CollegeName = dr["CollegeName"].ToString(),  
                        AccessionNo = Convert.ToInt64(dr["AccessionNo"]),
                        Name = dr["WhomIssued"].ToString(),
                        IDNo = dr["IDNo"].ToString(),
                        Title = dr["Title"].ToString(),
                        DateOfIssue = dr["IssueDate"],
                        LastReturnDate = dr["LastReturnDate"],
                        Type = dr["Type"].ToString(),
                        Author = dr["Author"].ToString(),
                        Discipline = dr["Discipline"].ToString()
                    };
                }
            }

            return null;
        }

        public object GetPhoto(string idNo)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();

            string sql = idNo.Length == 10
                ? "SELECT Snap FROM Admissions WHERE IDNO=@IDNO"
                : "SELECT Snap FROM Staff WHERE IDNO=@IDNO";

            using SqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("@IDNO", idNo);

            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read() && !dr.IsDBNull(0))
                return File((byte[])dr[0], "image/jpeg");

            return NotFound();
        }
        private void Renew(long accessionNo)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = @"UPDATE IssueRegister 
                       SET LastReturnDate=@LastReturnDate 
                       WHERE AccessionNo=@AccessionNo 
                      ";

                SqlCommand cmd = new SqlCommand(sql, con);

                cmd.Parameters.AddWithValue("@LastReturnDate", DateTime.Now.AddDays(7)); // same dtpRenewalDate
                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);

                con.Open();
                cmd.ExecuteNonQuery();
            }
        }
        private string GetUserFromSignature(string signature)
        {
            using SqlConnection con = new(_connectionString);
            con.Open();

            string sql = @"SELECT TOP 1 UserName
                   FROM UserMaster
                   WHERE Password = @Password
                   AND LoginType IN ('Admin','Staff')
                   AND ApplicationType = 'Windows'
                   AND ApplicationName = 'Library'";

            using SqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("@Password", signature);

            object result = cmd.ExecuteScalar();

            return result?.ToString() ?? "";
        }
        private string GetStaffNameById(SqlConnection con, long idNo)
        {
            string sql = "SELECT TOP 1 Name FROM Staff WHERE IDNo = @ID";

            using SqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("@ID", idNo);

            object result = cmd.ExecuteScalar();

            return result?.ToString() ?? "";
        }
        private void AddTransaction(dynamic info, string signature)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            con.Open();

            string userName = GetUserFromSignature(signature);

            if (!long.TryParse(userName, out long userId))
                throw new Exception("Invalid User");

            string staffName = GetStaffNameById(con, userId);

        string sql = @"INSERT INTO Transactions 
                 (ID, CollegeName, TransactionDate, TransactionTime, TransactionName, Type, 
                  AccessionNo, Title, IDNo, PersonName, PersonType, RenewalDate, UserID, UserName)
                   VALUES
                  (@ID, @CollegeName, @TransactionDate, @TransactionTime, @TransactionName, @Type,
                  @AccessionNo, @Title, @IDNo, @PersonName, @PersonType, @RenewalDate, @UserID, @UserName)";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@ID", MaxID());
            cmd.Parameters.AddWithValue("@CollegeName", info.CollegeName);
            cmd.Parameters.AddWithValue("@AccessionNo", info.AccessionNo);
            cmd.Parameters.AddWithValue("@TransactionDate", DateTime.Now);
            cmd.Parameters.AddWithValue("@TransactionTime", DateTime.Now);
            cmd.Parameters.AddWithValue("@TransactionName", "Renew");
            cmd.Parameters.AddWithValue("@Type", "Book");
            cmd.Parameters.AddWithValue("@Title", info.Title ?? "");
            cmd.Parameters.AddWithValue("@IDNo", info.IDNo ?? "");
            cmd.Parameters.AddWithValue("@PersonName", info.Name ?? "");
            cmd.Parameters.AddWithValue("@PersonType", info.Type ?? "");
            cmd.Parameters.AddWithValue("@RenewalDate", DateTime.Now);

            cmd.Parameters.Add("@UserID", SqlDbType.BigInt).Value = userId;
            cmd.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = staffName;

            cmd.ExecuteNonQuery();
        }
        private long MaxID()
        {
            long id = 1;

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = "SELECT MAX(ID) FROM Transactions";

                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();

                var result = cmd.ExecuteScalar();

                if (result != null && result != DBNull.Value)
                    id = Convert.ToInt64(result) + 1;
            }

            return id;
        }
        private bool CheckSignature(string signature)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM UserMaster WHERE Password=@Password";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@Password", signature);

                con.Open();
                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }
        //private object GetStudentStaffDetail(string idNo)
        //{
        //    using SqlConnection con = new SqlConnection(_connectionString);
        //    con.Open();

        //    string sql = "";

        //    if (idNo.Length == 6) // Staff
        //    {
        //        sql = "SELECT Designation, Department FROM Staff WHERE IDNo=@IDNo";
        //    }
        //    else // Student
        //    {
        //        sql = "SELECT Course, Batch FROM Admissions WHERE IDNo=@IDNo";
        //    }

        //    using SqlCommand cmd = new SqlCommand(sql, con);
        //    cmd.Parameters.AddWithValue("@IDNo", idNo);

        //    using SqlDataReader dr = cmd.ExecuteReader();

        //    //if (dr.Read())
        //    //{
        //    //    return new
        //    //    {
        //    //        Course = dr[0].ToString(),
        //    //        Batch = dr[1].ToString()
        //    //    };
        //    //}

        //    return new
        //    {
        //        Course = "",
        //        Batch = ""
        //    };
        //}
    }
}
