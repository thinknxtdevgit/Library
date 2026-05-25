using lib.DtoModel.ReturnBookDto;
using lib.Interface;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Service
{
    public class ReturnBookService :BaseService, IReturnBookService
    {
        private readonly string _connectionString;

        public ReturnBookService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor):base(httpContextAccessor)

        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");

        }

        // =====================================================
        // RECEIVE BOOK
        // =====================================================

        public async Task<ReceiveBookResponseDto>
            ReceiveBookAsync(
            ReceiveBookRequestDto req)
        {
            try
            {
                if (req.AccessionNo == 0)
                {
                    return new ReceiveBookResponseDto
                    {
                        Success = false,
                        Message = "Accession No required"
                    };
                }

                using SqlConnection con =
                    new SqlConnection(_connectionString);

                await con.OpenAsync();

                string sql = $@"
                    SELECT
                        WhomIssued,
                        IDNo,
                        Title,
                        IssueDate,
                        LastReturnDate,
                        CollegeName,
                        Type,
                        Author
                    FROM IssueRegister
                    WHERE AccessionNo=@Acc and CollegeName IN ({GetCollegeFilter()})";

                using SqlCommand cmd =
                    new SqlCommand(sql, con);

                cmd.Parameters.Add(
                    "@Acc",
                    SqlDbType.BigInt
                ).Value = req.AccessionNo;

                using SqlDataReader dr =
                    await cmd.ExecuteReaderAsync();

                if (!await dr.ReadAsync())
                {
                    return new ReceiveBookResponseDto
                    {
                        Success = false,
                        Message = "No record found"
                    };
                }

                string name =
                    dr["WhomIssued"].ToString();

                long idNo =
                    Convert.ToInt64(dr["IDNo"]);

                string title =
                    dr["Title"].ToString();

                string type =
                    dr["Type"].ToString();

                string author =
                    dr["Author"].ToString();

                string college =
                    dr["CollegeName"].ToString();

                DateTime issueDate =
                    Convert.ToDateTime(
                        dr["IssueDate"]);

                DateTime lastReturnDate =
                    Convert.ToDateTime(
                        dr["LastReturnDate"]);

                dr.Close();

                var extra =
                    await GetStudentStaffDetailAsync(
                        idNo.ToString());

                string snap =
                    await GetPhotoAsync(
                        idNo.ToString());

                // ====================================
                // VIEW MODE
                // ====================================

                if (string.IsNullOrWhiteSpace(
                    req.Signature))
                {
                    return new ReceiveBookResponseDto
                    {
                        Success = true,
                        Mode = "view",

                        Data =
                            new ReceiveBookDataDto
                            {
                                Name = name,
                                IdNo = idNo,
                                Title = title,
                                Type = type,
                                Author = author,
                                IssueDate = issueDate,
                                LastReturnDate =
                                    lastReturnDate,
                                College = college,
                                ExtraDetail = extra,
                                Snap = snap
                            }
                    };
                }

                // ====================================
                // SIGNATURE VALIDATION
                // ====================================
                var loginUser =
                    await GetUserFromSignatureAsync(
                        req.Signature);

                if (loginUser == null ||
                    loginUser.UserId == 0)
                {
                    return new ReceiveBookResponseDto
                    {
                        Success = false,
                        Message = "Invalid Signature"
                    };
                }
                // ====================================
                // DELETE ISSUE
                // ====================================

                await DeleteIssueAsync(
                    con,
                    req.AccessionNo,
                    college,
                    idNo);

                // ====================================
                // INSERT TRANSACTION
                // ====================================

                await InsertTransactionAsync(
    con,
    req.AccessionNo,
    college,
    title,
    idNo,
    name,
    type,
    loginUser.UserId,
    loginUser.StaffName);

                // ====================================
                // INSERT FINE
                // ====================================

                await InsertFineAsync(
     con,
     req.AccessionNo,
     college,
     title,
     idNo,
     name,
     type,
     author,
     issueDate,
     lastReturnDate,
     loginUser.UserId);
                return new ReceiveBookResponseDto
                {
                    Success = true,
                    Mode = "receive",
                    Message =
                        "Book received successfully"
                };
            }
            catch (Exception ex)
            {
                return new ReceiveBookResponseDto
                {
                    Success = false,
                    Message = ex.Message
                };
            }
        }

        // =====================================================
        // USER DETAIL
        // =====================================================

        private async Task<UserExtraDetailDto>
            GetStudentStaffDetailAsync(
            string idNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql =
                idNo.Length == 6
                ?
                $@"SELECT Designation,Department
                  FROM Staff
                  WHERE IDNo=@IDNo and CollegeName IN ({GetCollegeFilter()})"
                :
               $@"SELECT Course,Batch
                  FROM Admissions
                  WHERE IDNo=@IDNo and CollegeName IN ({GetCollegeFilter()})";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@IDNo",
                idNo);

            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                return new UserExtraDetailDto
                {
                    Course =
                        idNo.Length == 6
                        ? dr["Designation"]
                            .ToString()
                        : dr["Course"]
                            .ToString(),

                    Batch =
                        idNo.Length == 6
                        ? dr["Department"]
                            .ToString()
                        : dr["Batch"]
                            .ToString()
                };
            }

            return new UserExtraDetailDto();
        }

        // =====================================================
        // PHOTO
        // =====================================================

        private async Task<string> GetPhotoAsync(
            string idNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql =
                idNo.Length == 10
                ?
                "SELECT Snap FROM Admissions WHERE IDNO=@IDNO "
                :
                "SELECT Snap FROM Staff WHERE IDNO=@IDNO)";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@IDNO",
                idNo);

            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync() &&
                !dr.IsDBNull(0))
            {
                byte[] image =
                    (byte[])dr[0];

                return
                    "data:image/jpeg;base64,"
                    +
                    Convert.ToBase64String(image);
            }

            return "";
        }


        // =====================================================
        // GET USER DETAIL FROM SIGNATURE
        // =====================================================

        private async Task<SignatureUserDto>
      GetUserFromSignatureAsync(string signature)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = $@"
SELECT TOP 1
    UserName,
    CollegeName
FROM UserMaster
WHERE Password=@Password
AND LoginType IN ('Admin','Staff')
AND ApplicationName='Library'
AND CollegeName IN ({GetCollegeFilter()})";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@Password",
                signature);

            SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (!await dr.ReadAsync())
            {
                dr.Close();

                return new SignatureUserDto();
            }

            string userName =
                dr["UserName"].ToString();

            string collegeName =
                dr["CollegeName"].ToString();

            dr.Close();

            // USERNAME = STAFF ID
            if (!long.TryParse(userName, out long userId))
            {
                return new SignatureUserDto();
            }

            // ======================================
            // GET STAFF NAME
            // ======================================

            string staffSql = @"
SELECT TOP 1 Name
FROM Staff
WHERE IDNo=@ID
AND CollegeName=@CollegeName";

            using SqlCommand staffCmd =
                new SqlCommand(staffSql, con);

            staffCmd.Parameters.AddWithValue(
                "@ID",
                userId);

            staffCmd.Parameters.AddWithValue(
                "@CollegeName",
                collegeName);

            object staffResult =
                await staffCmd.ExecuteScalarAsync();

            return new SignatureUserDto
            {
                UserId = userId,
                UserName = userName,
                StaffName =
                    staffResult?.ToString() ?? ""
            };
        }

        // =====================================================
        // DELETE ISSUE
        // =====================================================

        private async Task DeleteIssueAsync(
            SqlConnection con,
            long accNo,
            string college,
            long idNo)
        {
            string sql = $@"
                DELETE FROM IssueRegister
                WHERE AccessionNo=@Acc
                AND CollegeName=@Col
                AND IDNo=@ID and CollegeName IN ({GetCollegeFilter()})";


            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@Acc",
                accNo);

            cmd.Parameters.AddWithValue(
                "@Col",
                college);

            cmd.Parameters.AddWithValue(
                "@ID",
                idNo);

            await cmd.ExecuteNonQueryAsync();
        }

        // =====================================================
        // TRANSACTION
        // =====================================================

        private async Task InsertTransactionAsync(
      SqlConnection con,
      long accNo,
      string college,
      string title,
      long idNo,
      string name,
      string type,
      long userId,
      string staffName)
        {
            long transactionId =
                await GetMaxTransactionIdAsync();

            string sql = @"
        INSERT INTO Transactions
        (
            ID,
            CollegeName,
            TransactionDate,
            TransactionTime,
            TransactionName,
            Type,
            AccessionNo,
            Title,
            IDNo,
            PersonName,
            PersonType,
            UserID,
            UserName
        )
        VALUES
        (
            @ID,
            @College,
            @Date,
            @Time,
            @TName,
            @Type,
            @Acc,
            @Title,
            @IDNo,
            @PName,
            @PType,
            @UID,
            @UName
        )";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@ID", transactionId);

            cmd.Parameters.AddWithValue("@College", college);

            cmd.Parameters.AddWithValue("@Date", DateTime.Now);

            cmd.Parameters.AddWithValue("@Time", DateTime.Now);

            cmd.Parameters.AddWithValue("@TName", "Return");

            cmd.Parameters.AddWithValue("@Type", "Book");

            cmd.Parameters.AddWithValue("@Acc", accNo);

            cmd.Parameters.AddWithValue("@Title", title);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            cmd.Parameters.AddWithValue("@PName", name);

            cmd.Parameters.AddWithValue("@PType", type);

            cmd.Parameters.AddWithValue("@UID", userId);

            cmd.Parameters.AddWithValue("@UName", staffName);

            await cmd.ExecuteNonQueryAsync();
        }
        // =====================================================
        // MAX TRANSACTION ID
        // =====================================================

        private async Task<long>
            GetMaxTransactionIdAsync()
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql =
                "SELECT ISNULL(MAX(ID),0) FROM Transactions";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            long max =
                Convert.ToInt64(
                    await cmd.ExecuteScalarAsync());

            return max + 1;
        }

        // =====================================================
        // INSERT FINE
        // =====================================================

        private async Task InsertFineAsync(
            SqlConnection con,
            long accNo,
            string college,
            string title,
            long idNo,
            string name,
            string type,
            string author,
            DateTime issueDate,
            DateTime lastReturnDate,
            long userId)
        {
            int days =
                (DateTime.Today -
                lastReturnDate.Date).Days;

            decimal fine = days > 0
                ? days * 5
                : 0;

            if (fine <= 0)
                return;

            string sql = @"
                INSERT INTO FineRegister
                (
                    CollegeName,
                    DateOfFine,
                    IDNo,
                    Name,
                    AccessionNo,
                    Title,
                    Author,
                    DateOfIssue,
                    LastReturnDate,
                    Fine,
                    Discipline,
                    UserID
                )
                VALUES
                (
                    @College,
                    @Date,
                    @IDNo,
                    @Name,
                    @Acc,
                    @Title,
                    @Author,
                    @Issue,
                    @Return,
                    @Fine,
                    @Type,
                    @User
                )";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@College",
                college);

            cmd.Parameters.AddWithValue(
                "@Date",
                DateTime.Now);

            cmd.Parameters.AddWithValue(
                "@IDNo",
                idNo);

            cmd.Parameters.AddWithValue(
                "@Name",
                name);

            cmd.Parameters.AddWithValue(
                "@Acc",
                accNo);

            cmd.Parameters.AddWithValue(
                "@Title",
                title);

            cmd.Parameters.AddWithValue(
                "@Author",
                author);

            cmd.Parameters.AddWithValue(
                "@Issue",
                issueDate);

            cmd.Parameters.AddWithValue(
                "@Return",
                lastReturnDate);

            cmd.Parameters.AddWithValue(
                "@Fine",
                fine);

            cmd.Parameters.AddWithValue(
                "@Type",
                type);

            cmd.Parameters.AddWithValue(
          "@User",
           userId);

            await cmd.ExecuteNonQueryAsync();
        }
    
     
    }
}