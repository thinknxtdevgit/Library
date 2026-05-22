//using lib.DtoModel.IssueBook;
//using lib.Interface;
//using Microsoft.Data.SqlClient;

//namespace lib.Service
//{
//    public class IssueBookService: IIssueBookService
//    {
//        private readonly string _connectionString;

//        public IssueBookService(IConfiguration configuration)
//        {
//            _connectionString =
//                configuration.GetConnectionString("DefaultConnection");
//        }

//        private int StudBookIssueDays = 7;
//        private int StaffBookIssueDays = 15;

//        public async Task<IssueBookResponseDto> CheckIdAsync(
//            IssueBookRequestDto request)
//        {
//            if (request == null || string.IsNullOrEmpty(request.txtidno))
//            {
//                return new IssueBookResponseDto
//                {
//                    Success = false,
//                    Message = "ID cannot be empty"
//                };
//            }

//            bool isStudent = request.txtidno.Length == 10;
//            bool isStaff = request.txtidno.Length == 6;

//            using SqlConnection con =
//                new SqlConnection(_connectionString);

//            await con.OpenAsync();

//            if (isStudent)
//            {
//                return await GetStudentDetail(
//                    con,
//                    request.txtidno,
//                    request.txtaccessionno);
//            }

//            if (isStaff)
//            {
//                return await GetStaffDetail(
//                    con,
//                    request.txtidno,
//                    request.txtaccessionno);
//            }

//            return new IssueBookResponseDto
//            {
//                Success = false,
//                Message = "Invalid ID Format"
//            };
//        }

//        // ================= STUDENT =================

//        private async Task<IssueBookResponseDto> GetStudentDetail(
//            SqlConnection con,
//            string idNo,
//            string accessionNo)
//        {
//            string sql = @"SELECT CollegeName,Snap,StudentName,
//                           Course,Batch,uniRollNo
//                           FROM Admissions
//                           WHERE IDNO=@IDNO";

//            using SqlCommand cmd = new SqlCommand(sql, con);

//            cmd.Parameters.AddWithValue("@IDNO", idNo);

//            SqlDataReader dr = await cmd.ExecuteReaderAsync();

//            if (!dr.Read())
//            {
//                return new IssueBookResponseDto
//                {
//                    Success = false,
//                    Message = "Invalid Student ID"
//                };
//            }

//            var collegeName = dr["CollegeName"].ToString();

//            UserDetailDto user = new UserDetailDto
//            {
//                CollegeName = collegeName,
//                Name = dr["StudentName"].ToString(),
//                Course = dr["Course"].ToString(),
//                Batch = dr["Batch"].ToString(),
//                UnivRollNo = dr["uniRollNo"].ToString(),
//                Type = "Student",
//                LastReturnDate =
//                    DateTime.Today.AddDays(StudBookIssueDays),

//                Image = dr["Snap"] != DBNull.Value
//                    ? "data:image/jpeg;base64," +
//                      Convert.ToBase64String((byte[])dr["Snap"])
//                    : null
//            };

//            dr.Close();

//            var previous =
//                await GetPreviousIssues(idNo, collegeName);

//            int totalBooks =
//                await GetIssuedBookCount(idNo, collegeName);
//            decimal totalFine =
//                await GetTotalFine(idNo);

//            BookDetailDto book = null;

//            if (!string.IsNullOrEmpty(accessionNo))
//            {
//                bool issued = IsBookAlreadyIssued(accessionNo);

//                if (issued)
//                {
//                    return new IssueBookResponseDto
//                    {
//                        Success = false,
//                        Message = "This book is already issued"
//                    };
//                }

//                book = await GetBookDetail(accessionNo);
//            }

//            return new IssueBookResponseDto
//            {
//                Success = true,
//                UserDetail = user,
//                PreviousIssues = previous,
//                TotalIssuedBooks = totalBooks,
//                BookDetail = book,
//                TotalFine = totalFine,
//            };
//        }

//        // ================= STAFF =================

//        private async Task<IssueBookResponseDto> GetStaffDetail(
//            SqlConnection con,
//            string idNo,
//            string accessionNo)
//        {
//            string sql = @"SELECT CollegeName,Snap,Name,
//                           Designation,Department,idno
//                           FROM Staff
//                           WHERE IDNO=@IDNO";

//            using SqlCommand cmd = new SqlCommand(sql, con);

//            cmd.Parameters.AddWithValue("@IDNO", idNo);

//            SqlDataReader dr = await cmd.ExecuteReaderAsync();

//            if (!dr.Read())
//            {
//                return new IssueBookResponseDto
//                {
//                    Success = false,
//                    Message = "Invalid Staff ID"
//                };
//            }

//            var collegeName = dr["CollegeName"].ToString();

//            UserDetailDto user = new UserDetailDto
//            {
//                CollegeName = collegeName,
//                Name = dr["Name"].ToString(),
//                Designation = dr["Designation"].ToString(),
//                Department = dr["Department"].ToString(),
//                Type = "Staff",
//                LastReturnDate =
//                    DateTime.Today.AddDays(StaffBookIssueDays),

//                Image = dr["Snap"] != DBNull.Value
//                    ? "data:image/jpeg;base64," +
//                      Convert.ToBase64String((byte[])dr["Snap"])
//                    : null
//            };

//            dr.Close();

//            var previous =
//                await GetPreviousIssues(idNo, collegeName);

//            int totalBooks =
//                await GetIssuedBookCount(idNo, collegeName);
//            decimal totalFine =
//    await GetTotalFine(idNo);

//            BookDetailDto book = null;

//            if (!string.IsNullOrEmpty(accessionNo))
//            {
//                bool issued = IsBookAlreadyIssued(accessionNo);

//                if (issued)
//                {
//                    return new IssueBookResponseDto
//                    {
//                        Success = false,
//                        Message = "This book is already issued"
//                    };
//                }

//                book = await GetBookDetail(accessionNo);
//            }

//            return new IssueBookResponseDto
//            {
//                Message="",
//                Success = true,
//                UserDetail = user,
//                PreviousIssues = previous,
//                TotalIssuedBooks = totalBooks,
//                BookDetail = book,
//                TotalFine = totalFine,
//            };
//        }

//        // ================= IsBookAlreadyIssued =================
//        private bool IsBookAlreadyIssued(string accessionNo)
//        {
//            using (SqlConnection con = new SqlConnection(_connectionString))
//            {
//                string sql = @"SELECT COUNT(*) 
//                       FROM IssueRegister 
//                       WHERE AccessionNo=@AccessionNo";

//                SqlCommand cmd = new SqlCommand(sql, con);

//                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);

//                con.Open();

//                int count = (int)cmd.ExecuteScalar();

//                return count > 0;
//            }
//        }

//        // ================= BOOK DETAIL =================

//        private async Task<BookDetailDto> GetBookDetail(
//            string accessionNo)
//        {
//            using SqlConnection con =
//                new SqlConnection(_connectionString);

//            await con.OpenAsync();

//            string sql = @"SELECT Title,Author,Category,
//                           AccessionNo
//                           FROM StockRegister
//                           WHERE AccessionNo=@AccessionNo";

//            using SqlCommand cmd = new SqlCommand(sql, con);

//            cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);

//            SqlDataReader dr = await cmd.ExecuteReaderAsync();

//            if (dr.Read())
//            {
//                return new BookDetailDto
//                {
//                    Success = true,
//                    AccessionNo =
//                        dr["AccessionNo"].ToString(),

//                    Title = dr["Title"].ToString(),

//                    Author = dr["Author"].ToString(),

//                    Category = dr["Category"].ToString()
//                };
//            }

//            return new BookDetailDto
//            {
//                Success = false,
//                Message = "Book not found"
//            };
//        }

//        // ================= PREVIOUS ISSUE =================

//        //private async Task<List<PreviousIssueDto>>
//        //    GetPreviousIssues(string idNo, string collegeName)
//        //{
//        //    List<PreviousIssueDto> list =
//        //        new List<PreviousIssueDto>();

//        //    using SqlConnection con =
//        //        new SqlConnection(_connectionString);

//        //    await con.OpenAsync();

//        //    string sql = @"SELECT Title,Author,AccessionNo,
//        //                   IssueDate,LastReturnDate,
//        //                   IDNo,Discipline
//        //                   FROM IssueRegister
//        //                   WHERE IDNo=@IDNo
//        //                   AND CollegeName=@CollegeName";

//        //    using SqlCommand cmd = new SqlCommand(sql, con);

//        //    cmd.Parameters.AddWithValue("@IDNo", idNo);

//        //    cmd.Parameters.AddWithValue("@CollegeName", collegeName);

//        //    SqlDataReader dr = await cmd.ExecuteReaderAsync();

//        //    while (dr.Read())
//        //    {
//        //        list.Add(new PreviousIssueDto
//        //        {
//        //            Title = dr["Title"].ToString(),

//        //            Author = dr["Author"].ToString(),

//        //            AccessionNo =
//        //                dr["AccessionNo"].ToString(),

//        //            IssueDate =
//        //                Convert.ToDateTime(dr["IssueDate"])
//        //                .ToString("dd/MM/yyyy"),

//        //            LastReturnDate =
//        //                Convert.ToDateTime(
//        //                    dr["LastReturnDate"])
//        //                .ToString("dd/MM/yyyy"),

//        //            IDNo = dr["IDNo"].ToString(),

//        //            Course = dr["Discipline"].ToString()
//        //        });
//        //    }

//        //    return list;
//        //}

//        private async Task<List<PreviousIssueDto>>
//    GetPreviousIssues(string idNo, string collegeName)
//        {
//            List<PreviousIssueDto> list =
//                new List<PreviousIssueDto>();

//            using SqlConnection con =
//                new SqlConnection(_connectionString);

//            await con.OpenAsync();

//            string sql = @"
//    SELECT
//        CONVERT(varchar(20),IssueDate,103) AS IssueDate,

//        CONVERT(varchar(10),IssueTime,108)
//        AS IssueTime,

//        IDNo,

//        Title,

//        Author,

//        AccessionNo,

//        CONVERT(varchar(20),LastReturnDate,103)
//        AS LastReturnDate,

//        Discipline,

//        Remarks,

//        WhomIssued,

//        Type,

//        CASE
//            WHEN DATEDIFF(day, LastReturnDate, GETDATE()) > 0
//            THEN DATEDIFF(day, LastReturnDate, GETDATE())
//            ELSE 0
//        END AS Days,

//        CASE
//            WHEN DATEDIFF(day, LastReturnDate, GETDATE()) > 0
//            THEN
//                DATEDIFF(day, LastReturnDate, GETDATE())
//                *
//                (
//                    SELECT FinePerDay
//                    FROM MasterFine
//                    WHERE CollegeName=@CollegeName
//                )
//            ELSE 0
//        END AS Fine

//    FROM IssueRegister

//    WHERE IDNo=@IDNo
//    AND CollegeName=@CollegeName

//    ORDER BY IssueDate DESC";

//            using SqlCommand cmd =
//                new SqlCommand(sql, con);

//            cmd.Parameters.AddWithValue("@IDNo", idNo);

//            cmd.Parameters.AddWithValue(
//                "@CollegeName",
//                collegeName);

//            SqlDataReader dr =
//                await cmd.ExecuteReaderAsync();

//            while (await dr.ReadAsync())
//            {
//                int days =
//                    Convert.ToInt32(dr["Days"]);

//                list.Add(new PreviousIssueDto
//                {
//                    IssueDate =
//                        dr["IssueDate"].ToString(),

//                    IssueTime =
//                        dr["IssueTime"].ToString(),

//                    IDNo =
//                        dr["IDNo"].ToString(),

//                    Title =
//                        dr["Title"].ToString(),

//                    Author =
//                        dr["Author"].ToString(),

//                    AccessionNo =
//                        dr["AccessionNo"].ToString(),

//                    LastReturnDate =
//                        dr["LastReturnDate"].ToString(),

//                    Course =
//                        dr["Discipline"].ToString(),

//                    Remarks =
//                        dr["Remarks"]?.ToString(),

//                    WhomIssued =
//                        dr["WhomIssued"]?.ToString(),

//                    Type =
//                        dr["Type"]?.ToString(),

//                    Days = days,

//                    Fine =
//                        Convert.ToDecimal(dr["Fine"]),

//                    IsOverDue = days > 0
//                });
//            }

//            return list;
//        }

//        // ================= TOTAL BOOKS =================

//        private async Task<int> GetIssuedBookCount(
//            string idNo,
//            string collegeName)
//        {
//            using SqlConnection con =
//                new SqlConnection(_connectionString);

//            await con.OpenAsync();

//            string sql = @"SELECT COUNT(*)
//                           FROM IssueRegister
//                           WHERE IDNo=@IDNo
//                           AND CollegeName=@CollegeName";

//            using SqlCommand cmd = new SqlCommand(sql, con);

//            cmd.Parameters.AddWithValue("@IDNo", idNo);

//            cmd.Parameters.AddWithValue(
//                "@CollegeName",
//                collegeName);

//            return (int)await cmd.ExecuteScalarAsync();
//        }

//        // ================= Calculate Fine =================
//        public async Task<decimal> CalculateFine(
//    string returnDate,
//    string collegeName,
//    string type)
//        {
//            if (string.IsNullOrEmpty(returnDate))
//                return 0;

//            DateTime lastReturnDate =
//                Convert.ToDateTime(returnDate);

//            int fineDays =
//                (DateTime.Today - lastReturnDate).Days;

//            if (fineDays <= 0)
//                return 0;

//            using SqlConnection con =
//                new SqlConnection(_connectionString);

//            await con.OpenAsync();

//            string sql = @"SELECT FinePerDay
//                   FROM MasterFine
//                   WHERE CollegeName=@CollegeName";

//            using SqlCommand cmd =
//                new SqlCommand(sql, con);

//            cmd.Parameters.AddWithValue(
//                "@CollegeName",
//                collegeName);

//            object result =
//                await cmd.ExecuteScalarAsync();

//            if (result == null)
//                return 0;

//            decimal finePerDay =
//                Convert.ToDecimal(result);

//            decimal totalFine =
//                finePerDay * fineDays;

//            if (type == "Staff")
//                totalFine = 0;

//            return totalFine;
//        }

//        // ================= InsertIntoFineRegister =================

//        public async Task InsertIntoFineRegister(
//           string collegeName,
//             string idNo,
//              string personName,
//               string accessionNo,
//              string title,
//              string author,
//              string issueDate,
//             string returnDate,
//            decimal totalFine,
//            string type,
//            string userName)
//        {
//            if (totalFine <= 0)
//                return;

//            using SqlConnection con =
//                new SqlConnection(_connectionString);

//            await con.OpenAsync();

//            string sql = @"
//    INSERT INTO FineRegister
//    (
//        CollegeName,
//        DateOfFine,
//        IDNo,
//        Name,
//        AccessionNo,
//        Title,
//        Author,
//        DateOfIssue,
//        LastReturnDate,
//        Fine,
//        Discipline,
//        UserID
//    )
//    VALUES
//    (
//        @CollegeName,
//        @DateOfFine,
//        @IDNo,
//        @Name,
//        @AccessionNo,
//        @Title,
//        @Author,
//        @DateOfIssue,
//        @LastReturnDate,
//        @Fine,
//        @Discipline,
//        @UserID
//    )";

//            using SqlCommand cmd =
//                new SqlCommand(sql, con);

//            cmd.Parameters.AddWithValue(
//                "@CollegeName",
//                collegeName);

//            cmd.Parameters.AddWithValue(
//                "@DateOfFine",
//                DateTime.Now);

//            cmd.Parameters.AddWithValue(
//                "@IDNo",
//                idNo);

//            cmd.Parameters.AddWithValue(
//                "@Name",
//                personName);

//            cmd.Parameters.AddWithValue(
//                "@AccessionNo",
//                accessionNo);

//            cmd.Parameters.AddWithValue(
//                "@Title",
//                title);

//            cmd.Parameters.AddWithValue(
//                "@Author",
//                author);

//            cmd.Parameters.AddWithValue(
//                "@DateOfIssue",
//                Convert.ToDateTime(issueDate));

//            cmd.Parameters.AddWithValue(
//                "@LastReturnDate",
//                Convert.ToDateTime(returnDate));

//            cmd.Parameters.AddWithValue(
//                "@Fine",
//                totalFine);

//            cmd.Parameters.AddWithValue(
//                "@Discipline",
//                type);

//            cmd.Parameters.AddWithValue(
//                "@UserID",
//                userName);

//            await cmd.ExecuteNonQueryAsync();
//        }
//        // ================= GetTotalFine =================
//        private async Task<decimal> GetTotalFine(string idNo)
//        {
//            using SqlConnection con =
//                new SqlConnection(_connectionString);

//            await con.OpenAsync();

//            string sql = @"
//    SELECT ISNULL(SUM(Fine),0)
//    FROM FineRegister
//    WHERE IDNo=@IDNo
//    AND FineStatus IS NULL";

//            using SqlCommand cmd =
//                new SqlCommand(sql, con);

//            cmd.Parameters.AddWithValue("@IDNo", idNo);

//            object result =
//                await cmd.ExecuteScalarAsync();

//            return Convert.ToDecimal(result);
//        }

//        public async Task<IssueBookResponseDto> IssueBookAsync(
//    IssueBookRequestDto request)
//        {
//            try
//            {
//                if (request == null)
//                {
//                    return new IssueBookResponseDto
//                    {
//                        Success = false,
//                        Message = "Invalid request"
//                    };
//                }

//                if (string.IsNullOrWhiteSpace(request.txtidno))
//                {
//                    return new IssueBookResponseDto
//                    {
//                        Success = false,
//                        Message = "ID No is required"
//                    };
//                }

//                if (string.IsNullOrWhiteSpace(request.txtaccessionno))
//                {
//                    return new IssueBookResponseDto
//                    {
//                        Success = false,
//                        Message = "Accession No is required"
//                    };
//                }

//                bool isStudent = request.txtidno.Length == 10;

//                bool isStaff = request.txtidno.Length == 6;

//                if (!isStudent && !isStaff)
//                {
//                    return new IssueBookResponseDto
//                    {
//                        Success = false,
//                        Message = "Invalid ID Format"
//                    };
//                }

//                // =========================
//                // BOOK ALREADY ISSUED
//                // =========================

//                bool alreadyIssued =
//                    IsBookAlreadyIssued(request.txtaccessionno);

//                if (alreadyIssued)
//                {
//                    return new IssueBookResponseDto
//                    {
//                        Success = false,
//                        Message = "This book is already issued"
//                    };
//                }

//                // =========================
//                // GET BOOK DETAIL
//                // =========================

//                BookDetailDto book =
//                    await GetBookDetail(request.txtaccessionno);

//                if (book == null || book.Success == false)
//                {
//                    return new IssueBookResponseDto
//                    {
//                        Success = false,
//                        Message = "Book not found in Stock Register"
//                    };
//                }

//                using SqlConnection con =
//                    new SqlConnection(_connectionString);

//                await con.OpenAsync();

//                UserDetailDto user;

//                string collegeName;

//                string type;

//                if (isStudent)
//                {
//                    string sql = @"SELECT CollegeName,
//                           StudentName,
//                           Course,
//                           Batch
//                           FROM Admissions
//                           WHERE IDNO=@IDNO";

//                    using SqlCommand cmd =
//                        new SqlCommand(sql, con);

//                    cmd.Parameters.AddWithValue(
//                        "@IDNO",
//                        request.txtidno);

//                    SqlDataReader dr =
//                        await cmd.ExecuteReaderAsync();

//                    if (!await dr.ReadAsync())
//                    {
//                        return new IssueBookResponseDto
//                        {
//                            Success = false,
//                            Message = "Student not found"
//                        };
//                    }

//                    collegeName =
//                        dr["CollegeName"].ToString();

//                    type = "Student";

//                    user = new UserDetailDto
//                    {
//                        Name =
//                            dr["StudentName"].ToString(),

//                        Course =
//                            dr["Course"].ToString(),

//                        Batch =
//                            dr["Batch"].ToString()
//                    };

//                    dr.Close();
//                }
//                else
//                {
//                    string sql = @"SELECT CollegeName,
//                           Name,
//                           Designation,
//                           Department
//                           FROM Staff
//                           WHERE IDNO=@IDNO";

//                    using SqlCommand cmd =
//                        new SqlCommand(sql, con);

//                    cmd.Parameters.AddWithValue(
//                        "@IDNO",
//                        request.txtidno);

//                    SqlDataReader dr =
//                        await cmd.ExecuteReaderAsync();

//                    if (!await dr.ReadAsync())
//                    {
//                        return new IssueBookResponseDto
//                        {
//                            Success = false,
//                            Message = "Staff not found"
//                        };
//                    }

//                    collegeName =
//                        dr["CollegeName"].ToString();

//                    type = "Staff";

//                    user = new UserDetailDto
//                    {
//                        Name =
//                            dr["Name"].ToString(),

//                        Designation =
//                            dr["Designation"].ToString(),

//                        Department =
//                            dr["Department"].ToString()
//                    };

//                    dr.Close();
//                }

//                // =========================
//                // INSERT ISSUE
//                // =========================

//                string insertSql = @"
//INSERT INTO IssueRegister
//(
//    CollegeName,
//    IssueDate,
//    IDNo,
//    Title,
//    Author,
//    AccessionNo,
//    WhomIssued,
//    LastReturnDate,
//    Discipline,
//    Type,
//    Category,
//    Remarks,
//    UserID,
//    IssueTime
//)
//VALUES
//(
//    @CollegeName,
//    @IssueDate,
//    @IDNo,
//    @Title,
//    @Author,
//    @AccessionNo,
//    @WhomIssued,
//    @LastReturnDate,
//    @Discipline,
//    @Type,
//    @Category,
//    @Remarks,
//    @UserID,
//    @IssueTime
//)";

//                using SqlCommand insertCmd =
//                    new SqlCommand(insertSql, con);

//                insertCmd.Parameters.AddWithValue(
//                    "@CollegeName",
//                    collegeName);

//                insertCmd.Parameters.AddWithValue(
//                    "@IssueDate",
//                    DateTime.Today);

//                insertCmd.Parameters.AddWithValue(
//                    "@IDNo",
//                    request.txtidno);

//                insertCmd.Parameters.AddWithValue(
//                    "@Title",
//                    book.Title ?? "");

//                insertCmd.Parameters.AddWithValue(
//                    "@Author",
//                    book.Author ?? "");

//                insertCmd.Parameters.AddWithValue(
//                    "@AccessionNo",
//                    request.txtaccessionno);

//                insertCmd.Parameters.AddWithValue(
//                    "@WhomIssued",
//                    user.Name ?? "");

//                int issueDays =
//                    type == "Student"
//                    ? StudBookIssueDays
//                    : StaffBookIssueDays;

//                insertCmd.Parameters.AddWithValue(
//                    "@LastReturnDate",
//                    DateTime.Today.AddDays(issueDays));

//                insertCmd.Parameters.AddWithValue(
//                    "@Discipline",
//                    type == "Student"
//                    ? user.Course ?? ""
//                    : user.Designation ?? "");

//                insertCmd.Parameters.AddWithValue(
//                    "@Type",
//                    type);

//                insertCmd.Parameters.AddWithValue(
//                    "@Category",
//                    book.Category ?? "");

//                insertCmd.Parameters.AddWithValue(
//                    "@Remarks",
//                    DBNull.Value);

//                insertCmd.Parameters.AddWithValue(
//                    "@UserID",
//                    "Admin");

//                insertCmd.Parameters.AddWithValue(
//                    "@IssueTime",
//                    DateTime.Now);

//                await insertCmd.ExecuteNonQueryAsync();

//                return new IssueBookResponseDto
//                {
//                    Success = true,
//                    Message = "Book issued successfully"
//                };
//            }
//            catch (Exception ex)
//            {
//                return new IssueBookResponseDto
//                {
//                    Success = false,
//                    Message = ex.Message
//                };
//            }
//        }
//    }
//}

using DocumentFormat.OpenXml.Wordprocessing;
using lib.DtoModel.IssueBook;
using lib.Interface;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Service
{
    public class IssueBookService : IIssueBookService
    {
        private readonly string _connectionString;

        private const int StudentIssueDays = 7;
        private const int StaffIssueDays = 15;

        public IssueBookService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection");
        }

        // ======================================================
        // CHECK ID
        // ======================================================

        public async Task<IssueBookResponseDto> CheckIdAsync(
            IssueBookRequestDto request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.txtidno))
            {
                return Fail("ID cannot be empty");
            }

            bool isStudent = request.txtidno.Length == 10;
            bool isStaff = request.txtidno.Length == 6;

            if (!isStudent && !isStaff)
            {
                return Fail("Invalid ID Format");
            }

            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            UserDetailDto user =
                await GetUserDetail(
                    con,
                    request.txtidno,
                    isStudent);

            if (user == null)
            {
                return Fail(
                    isStudent
                    ? "Invalid Student ID"
                    : "Invalid Staff ID");
            }

            List<PreviousIssueDto> previousIssues =
                await GetPreviousIssues(
                    request.txtidno,
                    user.CollegeName);

            int totalBooks =
                await GetIssuedBookCount(
                    request.txtidno,
                    user.CollegeName);

            decimal totalFine =
                await GetTotalFine(request.txtidno);

            BookDetailDto book = null;

            if (!string.IsNullOrWhiteSpace(
                request.txtaccessionno))
            {
                bool alreadyIssued =
                    await IsBookAlreadyIssuedAsync(
                        request.txtaccessionno);

                if (alreadyIssued)
                {
                    return Fail(
                        "This book is already issued");
                }

                book =
                    await GetBookDetail(
                        request.txtaccessionno);

                if (book == null ||
                    !book.Success)
                {
                    return Fail(
                        "Book not found");
                }
            }

            return new IssueBookResponseDto
            {
                Success = true,
                UserDetail = user,
                PreviousIssues = previousIssues,
                TotalIssuedBooks = totalBooks,
                TotalFine = totalFine,
                BookDetail = book
            };
        }

        // ======================================================
        // ISSUE BOOK
        // ======================================================

        public async Task<IssueBookResponseDto> IssueBookAsync(
            IssueBookRequestDto request)
        {
            try
            {
                if (request == null)
                    return Fail("Invalid Request");

                if (string.IsNullOrWhiteSpace(
                    request.txtidno))
                {
                    return Fail("ID No is required");
                }

                if (string.IsNullOrWhiteSpace(
                    request.txtaccessionno))
                {
                    return Fail("Accession No is required");
                }

                bool isStudent =
                    request.txtidno.Length == 10;

                bool isStaff =
                    request.txtidno.Length == 6;

                // ======================================
                // SIGNATURE VALIDATION
                // ======================================

                string userName =
                    await GetUserNameFromSignatureAsync(
                        request.signature);

                if (string.IsNullOrWhiteSpace(userName))
                {
                    return new IssueBookResponseDto
                    {
                        Success = false,
                        Message = "Invalid admin/staff signature"
                    };
                }

                if (!isStudent && !isStaff)
                {
                    return Fail("Invalid ID Format");
                }

                bool alreadyIssued =
                    await IsBookAlreadyIssuedAsync(
                        request.txtaccessionno);

                if (alreadyIssued)
                {
                    return Fail(
                        "This book is already issued");
                }

                BookDetailDto book =
                    await GetBookDetail(
                        request.txtaccessionno);

                if (book == null || !book.Success)
                {
                    return Fail(
                        "Book not found in Stock Register");
                }

                using SqlConnection con =
                    new SqlConnection(_connectionString);

                await con.OpenAsync();

                UserDetailDto user =
                    await GetUserDetail(
                        con,
                        request.txtidno,
                        isStudent);

                if (user == null)
                {
                    return Fail(
                        "User not found");
                }

                int issueDays =
                    isStudent
                    ? StudentIssueDays
                    : StaffIssueDays;

                string discipline =
                    isStudent
                    ? user.Course
                    : user.Designation;

                string type =
                    isStudent
                    ? "Student"
                    : "Staff";

                string insertSql = @"
INSERT INTO IssueRegister
(
    CollegeName,
    IssueDate,
    IDNo,
    Title,
    Author,
    AccessionNo,
    WhomIssued,
    LastReturnDate,
    Discipline,
    Type,
    Category,
    Remarks,
    UserID,
    IssueTime
)
VALUES
(
    @CollegeName,
    @IssueDate,
    @IDNo,
    @Title,
    @Author,
    @AccessionNo,
    @WhomIssued,
    @LastReturnDate,
    @Discipline,
    @Type,
    @Category,
    @Remarks,
    @UserID,
    @IssueTime
)";

                using SqlCommand cmd =
                    new SqlCommand(insertSql, con);

                cmd.Parameters.AddWithValue(
                    "@CollegeName",
                    user.CollegeName);

                cmd.Parameters.AddWithValue(
                    "@IssueDate",
                    DateTime.Today);

                cmd.Parameters.AddWithValue(
                    "@IDNo",
                    request.txtidno);

                cmd.Parameters.AddWithValue(
                    "@Title",
                    book.Title ?? "");

                cmd.Parameters.AddWithValue(
                    "@Author",
                    book.Author ?? "");

                cmd.Parameters.AddWithValue(
                    "@AccessionNo",
                    request.txtaccessionno);

                cmd.Parameters.AddWithValue(
                    "@WhomIssued",
                    user.Name ?? "");

                cmd.Parameters.AddWithValue(
                    "@LastReturnDate",
                    DateTime.Today.AddDays(issueDays));

                cmd.Parameters.AddWithValue(
                    "@Discipline",
                    discipline ?? "");

                cmd.Parameters.AddWithValue(
                    "@Type",
                    type);

                cmd.Parameters.AddWithValue(
                    "@Category",
                    book.Category ?? "");

                cmd.Parameters.AddWithValue(
                    "@Remarks",
                    DBNull.Value);

                cmd.Parameters.Add(
             "@UserID",
              SqlDbType.NVarChar
               ).Value = userName;

                cmd.Parameters.AddWithValue(
                    "@IssueTime",
                    DateTime.Now);

                await cmd.ExecuteNonQueryAsync();

                return Success(
                    "Book issued successfully");
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }

        // ======================================================
        // USER DETAIL
        // ======================================================

        private async Task<UserDetailDto> GetUserDetail(
            SqlConnection con,
            string idNo,
            bool isStudent)
        {
            string sql;

            if (isStudent)
            {
                sql = @"
SELECT
    CollegeName,
    Snap,
    StudentName,
    Course,
    Batch,
    uniRollNo
FROM Admissions
WHERE IDNO=@IDNO";
            }
            else
            {
                sql = @"
SELECT
    CollegeName,
    Snap,
    Name,
    Designation,
    Department
FROM Staff
WHERE IDNO=@IDNO";
            }

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@IDNO",
                idNo);

            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (!await dr.ReadAsync())
            {
                dr.Close();
                return null;
            }

            UserDetailDto user =
                new UserDetailDto
                {
                    CollegeName =
                        dr["CollegeName"].ToString(),

                    Name =
                        isStudent
                        ? dr["StudentName"].ToString()
                        : dr["Name"].ToString(),

                    Course =
                        isStudent
                        ? dr["Course"].ToString()
                        : null,

                    Batch =
                        isStudent
                        ? dr["Batch"].ToString()
                        : null,

                    Designation =
                        !isStudent
                        ? dr["Designation"].ToString()
                        : null,

                    Department =
                        !isStudent
                        ? dr["Department"].ToString()
                        : null,

                    UnivRollNo =
                        isStudent
                        ? dr["uniRollNo"].ToString()
                        : null,

                    Type =
                        isStudent
                        ? "Student"
                        : "Staff",

                    LastReturnDate =
                        DateTime.Today.AddDays(
                            isStudent
                            ? StudentIssueDays
                            : StaffIssueDays),

                    Image =
                        dr["Snap"] != DBNull.Value
                        ? "data:image/jpeg;base64," +
                          Convert.ToBase64String(
                              (byte[])dr["Snap"])
                        : null
                };

            dr.Close();

            return user;
        }

        // ======================================================
        // BOOK DETAIL
        // ======================================================

        private async Task<BookDetailDto> GetBookDetail(
            string accessionNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"
SELECT
    Title,
    Author,
    Category,
    AccessionNo
FROM StockRegister
WHERE AccessionNo=@AccessionNo";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@AccessionNo",
                accessionNo);

            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (!await dr.ReadAsync())
            {
                return new BookDetailDto
                {
                    Success = false,
                    Message = "Book not found"
                };
            }

            return new BookDetailDto
            {
                Success = true,
                AccessionNo =
                    dr["AccessionNo"].ToString(),

                Title =
                    dr["Title"].ToString(),

                Author =
                    dr["Author"].ToString(),

                Category =
                    dr["Category"].ToString()
            };
        }

        // ======================================================
        // BOOK ALREADY ISSUED
        // ======================================================

        private async Task<bool> IsBookAlreadyIssuedAsync(
            string accessionNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"
SELECT COUNT(*)
FROM IssueRegister
WHERE AccessionNo=@AccessionNo";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@AccessionNo",
                accessionNo);

            int count =
                (int)await cmd.ExecuteScalarAsync();

            return count > 0;
        }

        // ======================================================
        // TOTAL BOOK COUNT
        // ======================================================

        private async Task<int> GetIssuedBookCount(
            string idNo,
            string collegeName)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"
SELECT COUNT(*)
FROM IssueRegister
WHERE IDNo=@IDNo
AND CollegeName=@CollegeName";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@IDNo",
                idNo);

            cmd.Parameters.AddWithValue(
                "@CollegeName",
                collegeName);

            return (int)await cmd.ExecuteScalarAsync();
        }

        // ======================================================
        // TOTAL FINE
        // ======================================================

        private async Task<decimal> GetTotalFine(
            string idNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"
SELECT ISNULL(SUM(Fine),0)
FROM FineRegister
WHERE IDNo=@IDNo
AND FineStatus IS NULL";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@IDNo",
                idNo);

            object result =
                await cmd.ExecuteScalarAsync();

            return Convert.ToDecimal(result);
        }

        // ======================================================
        // PREVIOUS ISSUES
        // ======================================================

        private async Task<List<PreviousIssueDto>>
            GetPreviousIssues(
            string idNo,
            string collegeName)
        {
            List<PreviousIssueDto> list =
                new();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"
SELECT
    CONVERT(varchar(20),IssueDate,103)
    AS IssueDate,

    CONVERT(varchar(10),IssueTime,108)
    AS IssueTime,

    IDNo,
    Title,
    Author,
    AccessionNo,

    CONVERT(varchar(20),LastReturnDate,103)
    AS LastReturnDate,

    Discipline,
    Remarks,
    WhomIssued,
    Type,

    CASE
        WHEN DATEDIFF(day,
            LastReturnDate,
            GETDATE()) > 0
        THEN DATEDIFF(day,
            LastReturnDate,
            GETDATE())
        ELSE 0
    END AS Days,

    CASE
        WHEN DATEDIFF(day,
            LastReturnDate,
            GETDATE()) > 0
        THEN
            DATEDIFF(day,
            LastReturnDate,
            GETDATE())
            *
            (
                SELECT FinePerDay
                FROM MasterFine
                WHERE CollegeName=@CollegeName
            )
        ELSE 0
    END AS Fine

FROM IssueRegister

WHERE IDNo=@IDNo
AND CollegeName=@CollegeName

ORDER BY IssueDate DESC";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@IDNo",
                idNo);

            cmd.Parameters.AddWithValue(
                "@CollegeName",
                collegeName);

            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                int days =
                    Convert.ToInt32(dr["Days"]);

                list.Add(new PreviousIssueDto
                {
                    IssueDate =
                        dr["IssueDate"].ToString(),

                    IssueTime =
                        dr["IssueTime"].ToString(),

                    IDNo =
                        dr["IDNo"].ToString(),

                    Title =
                        dr["Title"].ToString(),

                    Author =
                        dr["Author"].ToString(),

                    AccessionNo =
                        dr["AccessionNo"].ToString(),

                    LastReturnDate =
                        dr["LastReturnDate"].ToString(),

                    Course =
                        dr["Discipline"].ToString(),

                    Remarks =
                        dr["Remarks"]?.ToString(),

                    WhomIssued =
                        dr["WhomIssued"]?.ToString(),

                    Type =
                        dr["Type"]?.ToString(),

                    Days = days,

                    Fine =
                        Convert.ToDecimal(dr["Fine"]),

                    IsOverDue =
                        days > 0
                });
            }

            return list;
        }

        // ======================================================
        // COMMON RESPONSE
        // ======================================================

        private IssueBookResponseDto Fail(
            string message)
        {
            return new IssueBookResponseDto
            {
                Success = false,
                Message = message
            };
        }

        private IssueBookResponseDto Success(
            string message)
        {
            return new IssueBookResponseDto
            {
                Success = true,
                Message = message
            };
        }
        // ========================GetUserNameFromSignatureAsync==============================

        private async Task<string> GetUserNameFromSignatureAsync(
    string signature)
        {
            if (string.IsNullOrWhiteSpace(signature))
                return string.Empty;

            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"
    SELECT TOP 1 UserName
    FROM UserMaster
    WHERE Password = @Password
    AND ApplicationName = 'Library'
    AND LoginType IN ('Admin','Staff')";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.Add(
                "@Password",
                SqlDbType.NVarChar
            ).Value = signature;

            object result =
                await cmd.ExecuteScalarAsync();

            return result?.ToString() ?? string.Empty;
        }
    }
}