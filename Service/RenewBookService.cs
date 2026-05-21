using lib.DtoModel.RenewBookDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class RenewBookService : IRenewBookService
    {
        private readonly string _connectionString;

        public RenewBookService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<RenewBookResponseDto> RenewBookAsync(
            RenewBookRequestDto request)
        {
            var response = new RenewBookResponseDto();

            if (request.AccessionNo == 0)
            {
                response.Success = false;
                response.Message = "Enter Accession No";

                return response;
            }

            var info = await GetBookInfoAsync(request.AccessionNo);

            if (info == null)
            {
                response.Success = false;
                response.Message = "Accession No does not exist";

                return response;
            }

            string snap = await GetPhotoAsync(info.IDNo);

            // ================= VIEW MODE =================

            if (string.IsNullOrWhiteSpace(request.Signature))
            {
                response.Success = true;
                response.Mode = "view";
                response.Data = info;
                response.Snap = snap;

                return response;
            }

            // ================= VALIDATION =================

            if (string.IsNullOrEmpty(info.Name))
            {
                response.Success = false;
                response.Message = "Invalid Name";

                return response;
            }

            if (string.IsNullOrEmpty(info.IDNo))
            {
                response.Success = false;
                response.Message = "Invalid ID No";

                return response;
            }

            if (string.IsNullOrEmpty(info.Title))
            {
                response.Success = false;
                response.Message = "Invalid Title";

                return response;
            }

            if (request.Signature.Contains("'"))
            {
                response.Success = false;
                response.Message = "Invalid Signature";

                return response;
            }

            if (info.LastReturnDate < DateTime.Now)
            {
                response.Success = false;
                response.Message = "Last Return Date Expired";

                return response;
            }

            bool validSignature =
                await CheckSignatureAsync(request.Signature);

            if (!validSignature)
            {
                response.Success = false;
                response.Message = "Invalid Signature";

                return response;
            }

            // ================= RENEW =================

            await RenewAsync(request.AccessionNo);

            await AddTransactionAsync(info, request.Signature);

            response.Success = true;
            response.Mode = "renew";
            response.Message = "Renewed Successfully";

            return response;
        }

        // =========================================
        // GET BOOK DETAIL
        // =========================================

        private async Task<RenewBookDetailDto?> GetBookInfoAsync(
            long accessionNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = @"SELECT
                           WhomIssued,
                           IDNo,
                           Title,
                           IssueDate,
                           LastReturnDate,
                           CollegeName,
                           Type,
                           AccessionNo,
                           Author,
                           Discipline
                           FROM IssueRegister
                           WHERE AccessionNo=@AccessionNo";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@AccessionNo",
                accessionNo);

            await con.OpenAsync();

            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                return new RenewBookDetailDto
                {
                    CollegeName = dr["CollegeName"].ToString(),

                    AccessionNo =
                        Convert.ToInt64(dr["AccessionNo"]),

                    Name = dr["WhomIssued"].ToString(),

                    IDNo = dr["IDNo"].ToString(),

                    Title = dr["Title"].ToString(),

                    DateOfIssue =
                        Convert.ToDateTime(dr["IssueDate"]),

                    LastReturnDate =
                        Convert.ToDateTime(dr["LastReturnDate"]),

                    Type = dr["Type"].ToString(),

                    Author = dr["Author"].ToString(),

                    Discipline = dr["Discipline"].ToString()
                };
            }

            return null;
        }

        // =========================================
        // PHOTO
        // =========================================

        private async Task<string?> GetPhotoAsync(string idNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = idNo.Length == 10
                ? "SELECT Snap FROM Admissions WHERE IDNO=@IDNO"
                : "SELECT Snap FROM Staff WHERE IDNO=@IDNO";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNO", idNo);

            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                if (!dr.IsDBNull(0))
                {
                    byte[] imageBytes = (byte[])dr[0];

                    return "data:image/jpeg;base64,"
                           + Convert.ToBase64String(imageBytes);
                }
            }

            return null;
        }

        // =========================================
        // RENEW BOOK
        // =========================================

        private async Task RenewAsync(long accessionNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = @"UPDATE IssueRegister
                           SET LastReturnDate=@LastReturnDate
                           WHERE AccessionNo=@AccessionNo";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@LastReturnDate",
                DateTime.Now.AddDays(7));

            cmd.Parameters.AddWithValue(
                "@AccessionNo",
                accessionNo);

            await con.OpenAsync();

            await cmd.ExecuteNonQueryAsync();
        }

        // =========================================
        // CHECK SIGNATURE
        // =========================================

        private async Task<bool> CheckSignatureAsync(
            string signature)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql =
                "SELECT COUNT(*) FROM UserMaster WHERE Password=@Password";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@Password",
                signature);

            await con.OpenAsync();

            int count =
                (int)await cmd.ExecuteScalarAsync();

            return count > 0;
        }

        // =========================================
        // GET USER
        // =========================================

        private async Task<string> GetUserFromSignatureAsync(
            string signature)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"SELECT TOP 1 UserName
                           FROM UserMaster
                           WHERE Password=@Password
                           AND LoginType IN ('Admin','Staff')
                           AND ApplicationType='Windows'
                           AND ApplicationName='Library'";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue(
                "@Password",
                signature);

            object result =
                await cmd.ExecuteScalarAsync();

            return result?.ToString() ?? "";
        }

        // =========================================
        // STAFF NAME
        // =========================================

        private async Task<string> GetStaffNameByIdAsync(
            SqlConnection con,
            long idNo)
        {
            string sql =
                "SELECT TOP 1 Name FROM Staff WHERE IDNo=@ID";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@ID", idNo);

            object result =
                await cmd.ExecuteScalarAsync();

            return result?.ToString() ?? "";
        }

        // =========================================
        // TRANSACTION
        // =========================================

        private async Task AddTransactionAsync(
            RenewBookDetailDto info,
            string signature)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string userName =
                await GetUserFromSignatureAsync(signature);

            if (!long.TryParse(userName, out long userId))
                throw new Exception("Invalid User");

            string staffName =
                await GetStaffNameByIdAsync(con, userId);

            string sql = @"INSERT INTO Transactions
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
                               RenewalDate,
                               UserID,
                               UserName
                           )
                           VALUES
                           (
                               @ID,
                               @CollegeName,
                               @TransactionDate,
                               @TransactionTime,
                               @TransactionName,
                               @Type,
                               @AccessionNo,
                               @Title,
                               @IDNo,
                               @PersonName,
                               @PersonType,
                               @RenewalDate,
                               @UserID,
                               @UserName
                           )";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@ID", await MaxIdAsync());

            cmd.Parameters.AddWithValue(
                "@CollegeName",
                info.CollegeName ?? "");

            cmd.Parameters.AddWithValue(
                "@TransactionDate",
                DateTime.Now);

            cmd.Parameters.AddWithValue(
                "@TransactionTime",
                DateTime.Now);

            cmd.Parameters.AddWithValue(
                "@TransactionName",
                "Renew");

            cmd.Parameters.AddWithValue(
                "@Type",
                "Book");

            cmd.Parameters.AddWithValue(
                "@AccessionNo",
                info.AccessionNo);

            cmd.Parameters.AddWithValue(
                "@Title",
                info.Title ?? "");

            cmd.Parameters.AddWithValue(
                "@IDNo",
                info.IDNo ?? "");

            cmd.Parameters.AddWithValue(
                "@PersonName",
                info.Name ?? "");

            cmd.Parameters.AddWithValue(
                "@PersonType",
                info.Type ?? "");

            cmd.Parameters.AddWithValue(
                "@RenewalDate",
                DateTime.Now);

            cmd.Parameters.AddWithValue(
                "@UserID",
                userId);

            cmd.Parameters.AddWithValue(
                "@UserName",
                staffName);

            await cmd.ExecuteNonQueryAsync();
        }

        // =========================================
        // MAX ID
        // =========================================

        private async Task<long> MaxIdAsync()
        {
            long id = 1;

            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = "SELECT MAX(ID) FROM Transactions";

            using SqlCommand cmd = new SqlCommand(sql, con);

            await con.OpenAsync();

            object result =
                await cmd.ExecuteScalarAsync();

            if (result != DBNull.Value && result != null)
            {
                id = Convert.ToInt64(result) + 1;
            }

            return id;
        }
    }
}

