    using DocumentFormat.OpenXml.Wordprocessing;
    using lib.DtoModel.IssueBook;
    using lib.Interface;
    using Microsoft.Data.SqlClient;
    using System.Data;

    namespace lib.Service
    {
        public class IssueBookService :BaseService,IIssueBookService
        {
            private readonly string _connectionString;
     

            private const int StudentIssueDays = 14;
            private const int StaffIssueDays = 15;

            public IssueBookService(
             IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor)
            : base(httpContextAccessor)
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
            int issueLimit =
            await GetIssueLimitAsync(
             user.CollegeName,
             user.Type);

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
                    BookDetail = book,
                    IssueLimit = issueLimit
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

                    string insertSql = $@"
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
                int currentIssuedBooks =
                await GetIssuedBookCount(request.txtidno, user.CollegeName);

                int issueLimit =
                    await GetIssueLimitAsync(user.CollegeName, user.Type);

                if (currentIssuedBooks >= issueLimit &&
                    !request.ForceIssue)
                {
                    return Fail(
                        $"Maximum is book limit is crossed. Maximum Allowed Books : {issueLimit}");
                }

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
                    sql = $@"
    SELECT
        CollegeName,
        Snap,
        StudentName,
        Course,
        Batch,
        uniRollNo
    FROM Admissions
    WHERE IDNO=@IDNO
    AND CollegeName IN ({GetCollegeFilter()})";
                }  
                else
                {
                    sql = $@"
    SELECT
        CollegeName,
        Snap,
        Name,
        Designation,
        Department
    FROM Staff
    WHERE IDNO=@IDNO
    AND CollegeName IN ({GetCollegeFilter()})";
                }

                using SqlCommand cmd =
                    new SqlCommand(sql, con);

                cmd.Parameters.AddWithValue(
                    "@IDNO",
                    idNo);

                //cmd.Parameters.AddWithValue(
                //    "@CollegeName",
                //    Colleges);

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

                string sql = $@"
    SELECT
        Title,
        Author,
        Category,
        AccessionNo
    FROM StockRegister
    WHERE AccessionNo=@AccessionNo
    AND CollegeName IN ({GetCollegeFilter()})";

                using SqlCommand cmd =
                    new SqlCommand(sql, con);

                cmd.Parameters.AddWithValue(
                    "@AccessionNo",
                    accessionNo);

                //cmd.Parameters.AddWithValue(
                //  "@CollegeName",
                // Colleges);

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

                string sql = $@"
    SELECT COUNT(*)
    FROM IssueRegister
    WHERE AccessionNo=@AccessionNo
    AND CollegeName IN ({GetCollegeFilter()})";

                using SqlCommand cmd =
                new SqlCommand(sql, con);

                cmd.Parameters.AddWithValue(
                    "@AccessionNo",
                    accessionNo);

                //cmd.Parameters.AddWithValue(
                //    "@CollegeName",
                //    Colleges);

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

                string sql = $@"
    SELECT COUNT(*)
    FROM IssueRegister
    WHERE IDNo=@IDNo
    AND CollegeName IN ({GetCollegeFilter()})";
          

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

                string sql = $@"
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
                string sql = $@"
    SELECT
        CONVERT(varchar(20),IssueDate,103) AS IssueDate,
        CONVERT(varchar(10),IssueTime,108) AS IssueTime,
        IDNo,
        Title,
        Author,
        AccessionNo,
        CONVERT(varchar(20),LastReturnDate,103) AS LastReturnDate,
        Discipline,
        Remarks,
        WhomIssued,
        Type,

        CASE
            WHEN DATEDIFF(day, LastReturnDate, GETDATE()) > 0
            THEN DATEDIFF(day, LastReturnDate, GETDATE())
            ELSE 0
        END AS Days,

        CASE
            WHEN DATEDIFF(day, LastReturnDate, GETDATE()) > 0
            THEN
                DATEDIFF(day, LastReturnDate, GETDATE())
                *
                (
                    SELECT TOP 1 FinePerDay
                    FROM MasterFine
                    WHERE CollegeName IN ({GetCollegeFilter()})
                )
            ELSE 0
        END AS Fine

    FROM IssueRegister
    WHERE IDNo=@IDNo
    AND CollegeName IN ({GetCollegeFilter()})

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

                string sql = $@"
    SELECT TOP 1 UserName
    FROM UserMaster
    WHERE Password=@Password
    AND CollegeName IN ({GetCollegeFilter()})
    AND ApplicationName='Library'
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

        // ========================CheckAccessionDetailAsync==============================
        public async Task<IssueBookResponseDto> CheckAccessionDetailAsync(string accessionNo, string collegeName)

        {
            try
            {
                if (string.IsNullOrWhiteSpace(accessionNo))
                {
                    return Fail("Accession No required");
                }

                using SqlConnection con =
                    new SqlConnection(_connectionString);

                await con.OpenAsync();
                string sql = @"
SELECT
    CollegeName,
    Title,
    Author,
    Category,
    AccessionNo
FROM StockRegister
WHERE AccessionNo = @AccessionNo";

                using SqlCommand cmd = new SqlCommand(sql, con);

                cmd.Parameters.Add("@AccessionNo", SqlDbType.NVarChar)
                    .Value = accessionNo;
                

                using SqlDataReader dr = await cmd.ExecuteReaderAsync();
                
                if (await dr.ReadAsync())
                {
                    string dbCollege = dr["CollegeName"]?.ToString() ?? "";

                    // 🚨 College validation (important)
                    if (!string.Equals(dbCollege, collegeName, StringComparison.OrdinalIgnoreCase))
                    {
                        return Fail($"Accession No doesn't belong to '{collegeName}'");
                    }

                    return new IssueBookResponseDto
                    {
                        Success = true,
                        Message = "Book Found",
                        BookDetail = new BookDetailDto
                        {
                            Success = true,
                            AccessionNo = dr["AccessionNo"]?.ToString(),
                            Title = dr["Title"]?.ToString(),
                            Author = dr["Author"]?.ToString(),
                            Category = dr["Category"]?.ToString()
                        }
                    };
                }
                else
                {
                    return Fail("Accession No does not exist");
                }
            }
            catch (Exception ex)
            {
                return Fail(ex.Message);
            }
        }
        // ========================CheckIssueLimit==============================
        private async Task<int> GetIssueLimitAsync(
    string collegeName,
    string personType)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"
    SELECT TOP 1 IssueLimit
    FROM MasterIssueLimit
    WHERE CollegeName = @CollegeName
    AND PersonType = @PersonType";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@CollegeName",
                collegeName);

            cmd.Parameters.AddWithValue(
                "@PersonType",
                personType);

            object result =
                await cmd.ExecuteScalarAsync();

            return result != null
                ? Convert.ToInt32(result)
                : 0;
        }
    }  }