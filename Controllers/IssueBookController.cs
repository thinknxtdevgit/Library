using lib.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System.Data;
using lib.Models.IssueBook;

namespace lib.Controllers
{
    public class IssueBookController : Controller
    {
        private readonly string _connectionString;

        public IssueBookController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ================= GLOBAL VARIABLES =================
        private string varUserName = "";
        private string varPassword = "";
        private string varLoginType = "";

        public int StudBookIssueDays = 7;
        public int StaffBookIssueDays = 15;
        private string varLibraryType = "Single";

    
        [HttpGet("/IssueBook")]

        public IActionResult IssueBook()
        {
            return View();
        }
        // ================= CHECK ID API =================

        [HttpPost]
        [Route("api/Login/checkid")]
        public IActionResult CheckId([FromBody] IssueBookRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.txtidno))
                return BadRequest("ID cannot be empty");

            bool rdbStudent = request.txtidno.Length == 10;
            bool rdbStaff = request.txtidno.Length == 6;

            //  ONLY ID
            if (string.IsNullOrEmpty(request.txtaccessionno))
            {
                var result = checkiddetail(request.txtidno, rdbStudent, rdbStaff, null);

                return Ok(new
                {
                    mode = "user",
                    data = result
                });
            }

            // ONLY BOOK DETAIL
            if (!string.IsNullOrEmpty(request.txtaccessionno)
                && string.IsNullOrEmpty(request.signature))
            {
                var book = checkAccessionDetail(request.txtaccessionno);

                return Ok(new
                {
                    success = true,
                    bookDetail = book
                });
            }

            // ISSUE BOOK
            varUserName = GetUserNameFromSignature(request.signature);

            if (string.IsNullOrEmpty(varUserName))
                return BadRequest("Invalid Signature"); 

            var result2 = checkiddetail(
                request.txtidno,
                rdbStudent,
                rdbStaff,
                request.txtaccessionno
            );

            return Ok(new
            {
                mode = "issue",
                data = result2
            });
        }

        private object checkiddetail(string txtidno, bool rdbStudent, bool rdbStaff, string txtaccessionno)
        {

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                // ================= STUDENT =================
                if (rdbStudent)
                {
                    DateTime lastReturnDate = DateTime.Today.AddDays(StudBookIssueDays);

                    string sql = @"SELECT CollegeName,Snap,StudentName,Course,Batch,uniRollNo 
                           FROM Admissions 
                           WHERE IDNO=@IDNO";

                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@IDNO", txtidno);

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        var collegeName = dr["CollegeName"].ToString();

                        var studentData = new
                        {
                            CollegeName = collegeName,
                            Name = dr["StudentName"].ToString(),
                            Course = dr["Course"].ToString(),
                            Batch = dr["Batch"].ToString(),
                            UnivRollNo = dr["uniRollNo"].ToString(),
                            Type = "Student",
                            LastReturnDate = lastReturnDate,
                            Image = dr["Snap"] != DBNull.Value
                               ? "data:image/jpeg;base64," + Convert.ToBase64String((byte[])dr["Snap"])
                               : null
                        };

                        dr.Close();

                        var previous = GetPreviousIssueDetail(txtidno, collegeName);
                        int totalBooks = GetIssuedBookCount(txtidno, collegeName);

                        object accession = null;

                        // Only when accessionNo provided
                        if (!string.IsNullOrEmpty(txtaccessionno))
                        {
                            accession = checkAccessionDetail(txtaccessionno);

                            bool issued = BookIssue(txtidno, collegeName, txtaccessionno, "Student");

                            if (!issued)
                            {
                                return new
                                {
                                    success = false,
                                    message = "This book is already issued"
                                };
                            }
                        }
                        return new
                        {
                            success = true,
                            data = studentData,
                            previousIssue = previous,
                            totalIssuedBooks = totalBooks,
                            accessionDetail = accession
                        };
                    }

                    return new { success = false, message = "Invalid Student ID" };
                }

                // ================= STAFF =================
                else if (rdbStaff)
                {
                    DateTime lastReturnDate = DateTime.Today.AddDays(StaffBookIssueDays);

                    string sql = @"SELECT CollegeName,Snap,Name,Designation,Department,idno 
                           FROM Staff 
                           WHERE IDNO=@IDNO";

                    SqlCommand cmd = new SqlCommand(sql, con);
                    cmd.Parameters.AddWithValue("@IDNO", txtidno);

                    SqlDataReader dr = cmd.ExecuteReader();

                    if (dr.Read())
                    {
                        var collegeName = dr["CollegeName"].ToString();

                        var staffData = new
                        {
                            CollegeName = collegeName,
                            Name = dr["Name"].ToString(),
                            Designation = dr["Designation"].ToString(),
                            Department = dr["Department"].ToString(),
                            IdNo = dr["idno"].ToString(),
                            Type = "Staff",
                            LastReturnDate = lastReturnDate,
                            Image = dr["Snap"] != DBNull.Value
                            ? "data:image/jpeg;base64," + Convert.ToBase64String((byte[])dr["Snap"])
                             : null
                        };

                        dr.Close();

                        var previous = GetPreviousIssueDetail(txtidno, collegeName);
                        int totalBooks = GetIssuedBookCount(txtidno, collegeName);

                        if (!string.IsNullOrEmpty(txtaccessionno))
                        {
                            bool issued = BookIssue(txtidno, collegeName, txtaccessionno, "Staff");

                            if (!issued)
                            {
                                return new
                                {
                                    success = false,
                                    message = "This book is already issued"
                                };
                            }
                        }
                        return new
                        {
                            success = true,
                            data = staffData,
                            previousIssue = previous,
                            totalIssuedBooks = totalBooks,
                        };
                    }

                    return new { success = false, message = "Invalid Staff ID" };
                }

                return new { success = false, message = "Invalid ID Format" };
            }
        }

        private int GetIssuedBookCount(string idNo, string collegeName)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            con.Open();

            string sql = @"SELECT COUNT(*) 
                   FROM IssueRegister 
                   WHERE IDNo = @IDNo AND CollegeName = @College";

            using SqlCommand cmd = new SqlCommand(sql, con);
            cmd.Parameters.AddWithValue("@IDNo", idNo);
            cmd.Parameters.AddWithValue("@College", collegeName);

            return (int)cmd.ExecuteScalar();
        }


        // ================= BOOK ISSUE (FIXED MISSING METHOD) =================
        private bool BookIssue(string idNo, string collegeName, string accessionNo, string type)
        {
            if (string.IsNullOrEmpty(accessionNo))
                return true;

            // Duplicate check
            if (IsBookAlreadyIssued(accessionNo))
            {
                return false;
            }

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                var book = GetBookFullDetail(accessionNo);
                var user = GetUserDetail(idNo);

                string sql = @"INSERT INTO IssueRegister
               (CollegeName, IssueDate, IDNo, Title, Author, AccessionNo, WhomIssued,
               LastReturnDate, Discipline, Type, Category, Remarks, UserID, IssueTime)
                VALUES
               (@CollegeName, @IssueDate, @IDNo, @Title, @Author, @AccessionNo, @WhomIssued,
               @LastReturnDate, @Discipline, @Type, @Category, @Remarks, @UserID, @IssueTime)";

                SqlCommand cmd = new SqlCommand(sql, con);

                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
                cmd.Parameters.AddWithValue("@IssueDate", DateTime.Today);
                cmd.Parameters.AddWithValue("@IDNo", idNo);

                //  If empty then NULL
                cmd.Parameters.AddWithValue("@Title", string.IsNullOrEmpty(book?.Title) ? DBNull.Value : book.Title);
                cmd.Parameters.AddWithValue("@Author", string.IsNullOrEmpty(book?.Author) ? DBNull.Value : book.Author);

                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);

                cmd.Parameters.AddWithValue("@WhomIssued", string.IsNullOrEmpty(user?.Name) ? DBNull.Value : user.Name);

                int days = (type == "Student") ? StudBookIssueDays : StaffBookIssueDays;
                cmd.Parameters.AddWithValue("@LastReturnDate", DateTime.Today.AddDays(days));

                //  Discipline = Course
                cmd.Parameters.AddWithValue("@Discipline",
                    type == "Student"
                        ? (object)(user?.Course ?? DBNull.Value)
                        : (object)(user?.Designation ?? DBNull.Value)
                );

                cmd.Parameters.AddWithValue("@Type", type);

                //  Category from StockRegister
                cmd.Parameters.AddWithValue("@Category", string.IsNullOrEmpty(book?.Category) ? DBNull.Value : book.Category);

                //  Remarks 
                cmd.Parameters.AddWithValue("@Remarks", DBNull.Value);

                // UserID 
                cmd.Parameters.AddWithValue("@UserID", varUserName);

                //  IssueTime 
                cmd.Parameters.AddWithValue("@IssueTime", DateTime.Now);

                cmd.ExecuteNonQuery();

            }

            return true;
        }

        private dynamic GetUserDetail(string idNo)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                string sql = idNo.Length == 10
                    ? "SELECT StudentName, Course FROM Admissions WHERE IDNo=@IDNo"
                    : "SELECT Name, Designation FROM Staff WHERE IDNo=@IDNo";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@IDNo", idNo);

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    var designation = "";

                    if (idNo.Length != 10) // staff
                    {
                        designation = dr["Designation"] == DBNull.Value
                            ? ""
                            : dr["Designation"].ToString();
                    }

                    return new
                    {
                        Name = idNo.Length == 10
                            ? dr["StudentName"].ToString()
                            : dr["Name"].ToString(),

                        Course = idNo.Length == 10
                            ? dr["Course"].ToString()
                            : null,

                        Designation = designation
                    };
                }
            }

            return null;
        }

        private string GetUserNameFromSignature(string signature)
        {
            if (string.IsNullOrEmpty(signature))
                return "";

            using SqlConnection con = new(_connectionString);
            con.Open();

            string sql = @"SELECT TOP 1 UserName 
                   FROM UserMaster 
                   WHERE Password = @Password
                   AND LoginType = 'Admin'
                   AND ApplicationName = 'Library'";

            using SqlCommand cmd = new(sql, con);
            cmd.Parameters.Add("@Password", SqlDbType.NVarChar).Value = signature;

            object result = cmd.ExecuteScalar();

            return result?.ToString() ?? "";
        }
        private bool IsBookAlreadyIssued(string accessionNo)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM IssueRegister WHERE AccessionNo=@AccessionNo";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);

                con.Open();
                int count = (int)cmd.ExecuteScalar();

                return count > 0;
            }
        }

        private dynamic GetBookFullDetail(string accessionNo)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                string sql = @"SELECT Title, Author, Category 
                       FROM StockRegister 
                       WHERE AccessionNo=@AccessionNo";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    return new
                    {
                        Title = dr["Title"].ToString(),
                        Author = dr["Author"].ToString(),
                        Category = dr["Category"].ToString()
                    };
                }
            }

            return null;
        }

        // ================= ACC SESSION DETAIL =================


        private object checkAccessionDetail(string accessionNo)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                string sql = @"SELECT Title,Author,Category,AccessionNo 
                       FROM StockRegister 
                       WHERE AccessionNo=@AccessionNo";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);

                SqlDataReader dr = cmd.ExecuteReader();

                if (dr.Read())
                {
                    return new
                    {
                        success = true,
                        AccessionNo = dr["AccessionNo"].ToString(),
                        Title = dr["Title"].ToString(),
                        Author = dr["Author"].ToString(),
                        Category = dr["Category"].ToString()
                    };
                }

                return new { success = false, message = "Book not found" };
            }
        }

        // ================= PREVIOUS ISSUE =================


        private object GetPreviousIssueDetail(string txtidno, string txtCollegeName)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                string sql = @"SELECT Title, Author, AccessionNo,
                       CONVERT(varchar(20),IssueDate,103) AS IssueDate,
                       CONVERT(varchar(20),LastReturnDate,103) AS LastReturnDate,IDNo,Discipline
                       FROM IssueRegister
                       WHERE IDNo=@IDNo AND CollegeName=@CollegeName
                       ORDER BY IssueDate DESC";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@IDNo", txtidno);
                cmd.Parameters.AddWithValue("@CollegeName", txtCollegeName);

                SqlDataReader dr = cmd.ExecuteReader();

                var list = new List<object>();

                while (dr.Read())
                {
                    list.Add(new
                    {
                        Title = dr["Title"].ToString(),
                        Author = dr["Author"].ToString(),
                        AccessionNo = dr["AccessionNo"].ToString(),
                        IssueDate = dr["IssueDate"].ToString(),
                        LastReturnDate = dr["LastReturnDate"].ToString(),
                        IDNo = dr["IDNo"].ToString(),
                        Coures = dr["Discipline"].ToString()
                        

                    });
                }

                return new
                {
                    Total = list.Count,
                    Data = list
                };
            }
        }

        // ================= TOTAL FINE =================
        private string getTotalFine(string idNo)
        {
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                con.Open();

                string sql = "SELECT ISNULL(SUM(fine),0) FROM FineRegister WHERE IDNo=@IDNo AND FineStatus IS NULL";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@IDNo", idNo);

                return cmd.ExecuteScalar().ToString();
            }
        }

        // ================= COLLEGE LIST =================
        public string GetAssignedCollegeName1()
        {
            var colleges = new List<string>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = @"SELECT CollegeName 
                               FROM UserMaster 
                               WHERE UserName=@UserName AND Password=@Password AND LoginType=@LoginType";

                SqlCommand cmd = new SqlCommand(sql, con);
                cmd.Parameters.AddWithValue("@UserName", varUserName ?? "");
                cmd.Parameters.AddWithValue("@Password", varPassword ?? "");
                cmd.Parameters.AddWithValue("@LoginType", varLoginType ?? "");

                con.Open();
                SqlDataReader dr = cmd.ExecuteReader();

                while (dr.Read())
                {
                    colleges.Add($"'{dr["CollegeName"]}'");
                }
            }

            return string.Join(",", colleges);
        }

        }
    }

    
