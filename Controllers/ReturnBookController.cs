using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Controllers
{
    public class ReturnBookController : Controller
    {
        private readonly string _connectionString;
        public ReturnBookController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        #region Models



        public class BookRequest
        {
            public long AccessionNo { get; set; }
            public string Signature { get; set; }  // optional
        }
        public class AccessionRequest
        {
            public long AccessionNo { get; set; }

        }

        public class ReceiveRequest
        {
            public long AccessionNo { get; set; }
            public string Signature { get; set; }
        }
        #endregion
        [HttpGet("/ReceiveBook")]
        public IActionResult ReturnBook()
        {
            return View();
        }
        [HttpPost("/ReceiveBook")]
        public IActionResult ReceiveBook([FromBody] BookRequest req)
        {
            if (req.AccessionNo == 0)
                return BadRequest("AccessionNo required");

            using SqlConnection con = new(_connectionString);
            con.Open();

            string sql = @"SELECT WhomIssued,IDNo,Title,IssueDate,LastReturnDate,
                   CollegeName,Type,Author
                   FROM IssueRegister
                   WHERE AccessionNo=@Acc";

            using SqlCommand cmd = new(sql, con);
            cmd.Parameters.Add("@Acc", SqlDbType.BigInt).Value = req.AccessionNo;


            using SqlDataReader dr = cmd.ExecuteReader();

            if (!dr.Read())
                return NotFound("No record found");

            string name = dr["WhomIssued"].ToString();
            long idNo = Convert.ToInt64(dr["IDNo"]);
            string title = dr["Title"].ToString();
            string type = dr["Type"].ToString();
            string author = dr["Author"].ToString();
            string college = dr["CollegeName"].ToString();
            DateTime issueDate = Convert.ToDateTime(dr["IssueDate"]);
            DateTime lastReturnDate = Convert.ToDateTime(dr["LastReturnDate"]);

            dr.Close();
            var extra = GetStudentStaffDetail(idNo.ToString());
            var Snap = GetPhoto(idNo.ToString());

            // ================= VIEW MODE =================
            if (string.IsNullOrWhiteSpace(req.Signature))
            {
                return Ok(new
                {
                    success = true,
                    mode = "view",
                    data = new
                    {
                        name,
                        idNo,
                        title,
                        type,
                        author,
                        issueDate,
                        lastReturnDate,
                        college,
                        extraDetail = extra,
                        snap = Snap
                    }

                });
            }

            // ================= RECEIVE MODE =================
            if (!CheckSignature(req.Signature))
                return BadRequest("Invalid Signature");

            string userNameFromLogin = GetUserFromSignature(req.Signature);

            if (!long.TryParse(userNameFromLogin, out long staffId))
                return BadRequest("Invalid User");

            string staffName = GetStaffNameById(con, staffId);

            if (string.IsNullOrEmpty(staffName))
                return BadRequest("Staff not found");

            DeleteIssue(con, req.AccessionNo, college, idNo);

            InsertTransaction(
                con,
                req.AccessionNo,
                college,
                title,
                idNo,
                name,
                type,
                staffId,
                staffName
            );

            InsertFine(con, req.AccessionNo, college, title, idNo, name, type, author, issueDate, lastReturnDate);

            return Ok(new
            {
                success = true,
                mode = "receive",
                message = "Book received successfully"
            });
        }


        private object GetStudentStaffDetail(string idNo)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            con.Open();

            string sql = "";

            if (idNo.Length == 6) // Staff
            {
                sql = "SELECT Designation, Department FROM Staff WHERE IDNo=@IDNo";
            }
            else // Student
            {
                sql = "SELECT Course, Batch FROM Admissions WHERE IDNo=@IDNo";
            }

            using SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@IDNo", idNo);

            using SqlDataReader dr = cmd.ExecuteReader();

            if (dr.Read())
            {

                // ✅ SAME STRUCTURE RETURN
                return new
                {
                    Course = idNo.Length == 6
                        ? dr["Designation"].ToString()   // Staff → Designation
                        : dr["Course"].ToString(),       // Student → Course

                    Batch = idNo.Length == 6
                        ? dr["Department"].ToString()    // Staff → Department
                        : dr["Batch"].ToString()
                    //if (idNo.Length == 10) // Student
                    //{
                    //    return new
                    //    {
                    //        Course = dr["Course"].ToString(),
                    //        Batch = dr["Batch"].ToString()
                    //    };
                    //}
                    //else // Staff
                    //{
                    //    return new
                    //    {
                    //        Discipline = dr["Designation"].ToString(),
                    //        Department =dr  ["Department"].ToString()
                    //    };
                };
            }
            return null;
        }
        // ✅ IMPORTANT (default return)
        //return new
        //{
        //    Course = "",
        //    Batch = ""
        //};
        //   }   
        #region PHOTO
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
        #endregion

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

        #region DATABASE OPERATIONS (OPTIMIZED)

        private void DeleteIssue(SqlConnection con, long accNo, string college, long idNo)
        {
            string sql = @"DELETE FROM IssueRegister
                           WHERE AccessionNo=@Acc AND CollegeName=@Col AND IDNo=@ID";

            using SqlCommand cmd = new(sql, con);

            cmd.Parameters.Add("@Acc", SqlDbType.BigInt).Value = accNo;
            cmd.Parameters.Add("@Col", SqlDbType.NVarChar).Value = college;
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = idNo;

            cmd.ExecuteNonQuery();
        }

        private void InsertTransaction(SqlConnection con, long accNo, string college,
            string title, long idNo, string name, string type, long userId, string userName)
        {
            string sql = @"INSERT INTO Transactions
                           (ID,CollegeName,TransactionDate,TransactionTime,TransactionName,
                            Type,AccessionNo,Title,IDNo,PersonName,PersonType,UserID,UserName)
                           VALUES
                           (@ID,@College,@Date,@Time,@Name,@Type,@Acc,@Title,@IDNo,@PName,@PType,@UID,@UName)";

            using SqlCommand cmd = new(sql, con);
            cmd.Parameters.Add("@ID", SqlDbType.BigInt).Value = GetMaxTransactionId();
            cmd.Parameters.Add("@College", SqlDbType.NVarChar).Value = college;
            cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = DateTime.Now;
            cmd.Parameters.Add("@Time", SqlDbType.DateTime).Value = DateTime.Now;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = "Return";
            cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = "Book";
            cmd.Parameters.Add("@Acc", SqlDbType.BigInt).Value = accNo;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar).Value = title;
            cmd.Parameters.Add("@IDNo", SqlDbType.BigInt).Value = idNo;
            cmd.Parameters.Add("@PName", SqlDbType.NVarChar).Value = name;
            cmd.Parameters.Add("@PType", SqlDbType.NVarChar).Value = type;

            cmd.Parameters.Add("@UID", SqlDbType.BigInt).Value = userId;
            cmd.Parameters.Add("@UName", SqlDbType.NVarChar).Value = userName;


            cmd.ExecuteNonQuery();
        }


        private long GetMaxTransactionId()
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = "SELECT ISNULL(MAX(ID),0) FROM Transactions";

                SqlCommand cmd = new SqlCommand(sql, con);
                con.Open();

                long maxId = Convert.ToInt64(cmd.ExecuteScalar());

                return maxId + 1;
            }
        }
        private void InsertFine(SqlConnection con, long accNo, string college,
            string title, long idNo, string name, string type, string author,
            DateTime issueDate, DateTime lastReturnDate)
        {
            string sql = @"INSERT INTO FineRegister
                           (CollegeName,DateOfFine,IDNo,Name,AccessionNo,Title,Author,
                            DateOfIssue,LastReturnDate,Fine,Discipline,UserID)
                           VALUES
                           (@College,@Date,@IDNo,@Name,@Acc,@Title,@Author,@Issue,@Return,@Fine,@Type,@User)";

            using SqlCommand cmd = new(sql, con);

            cmd.Parameters.Add("@College", SqlDbType.NVarChar).Value = college;
            cmd.Parameters.Add("@Date", SqlDbType.DateTime).Value = DateTime.Now;
            cmd.Parameters.Add("@IDNo", SqlDbType.BigInt).Value = idNo;
            cmd.Parameters.Add("@Name", SqlDbType.NVarChar).Value = name;
            cmd.Parameters.Add("@Acc", SqlDbType.BigInt).Value = accNo;
            cmd.Parameters.Add("@Title", SqlDbType.NVarChar).Value = title;
            cmd.Parameters.Add("@Author", SqlDbType.NVarChar).Value = author;
            cmd.Parameters.Add("@Issue", SqlDbType.DateTime).Value = issueDate;
            cmd.Parameters.Add("@Return", SqlDbType.DateTime).Value = lastReturnDate;
            cmd.Parameters.Add("@Fine", SqlDbType.Int).Value = 0;
            cmd.Parameters.Add("@Type", SqlDbType.NVarChar).Value = type;
            cmd.Parameters.Add("@User", SqlDbType.BigInt).Value = 1;

            cmd.ExecuteNonQuery();
        }

        #endregion
    }

}






