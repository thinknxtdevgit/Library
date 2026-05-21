using lib.DtoModel.ReturnBookDto;
using lib.Interface;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Service
{
    public class ReturnBookService: IReturnBookService
    {
        private readonly string _connectionString;

        public ReturnBookService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<ReceiveBookResponseDto> ReceiveBookAsync(
            ReceiveBookRequestDto req)
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

            string sql = @"SELECT WhomIssued,IDNo,Title,
                           IssueDate,LastReturnDate,
                           CollegeName,Type,Author
                           FROM IssueRegister
                           WHERE AccessionNo=@Acc";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.Add("@Acc",
                SqlDbType.BigInt).Value = req.AccessionNo;

            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (!dr.Read())
            {
                return new ReceiveBookResponseDto
                {
                    Success = false,
                    Message = "No record found"
                };
            }

            string name = dr["WhomIssued"].ToString();
            long idNo = Convert.ToInt64(dr["IDNo"]);
            string title = dr["Title"].ToString();
            string type = dr["Type"].ToString();
            string author = dr["Author"].ToString();
            string college = dr["CollegeName"].ToString();

            DateTime issueDate =
                Convert.ToDateTime(dr["IssueDate"]);

            DateTime lastReturnDate =
                Convert.ToDateTime(dr["LastReturnDate"]);

            dr.Close();

            var extra = await GetStudentStaffDetail(idNo.ToString());

            string snap = await GetPhoto(idNo.ToString());

            // VIEW MODE
            if (string.IsNullOrWhiteSpace(req.Signature))
            {
                return new ReceiveBookResponseDto
                {
                    Success = true,
                    Mode = "view",

                    Data = new ReceiveBookDataDto
                    {
                        Name = name,
                        IdNo = idNo,
                        Title = title,
                        Type = type,
                        Author = author,
                        IssueDate = issueDate,
                        LastReturnDate = lastReturnDate,
                        College = college,
                        ExtraDetail = extra,
                        Snap = snap
                    }
                };
            }

            // SIGNATURE CHECK
            bool isValid =
                await CheckSignature(req.Signature);

            if (!isValid)
            {
                return new ReceiveBookResponseDto
                {
                    Success = false,
                    Message = "Invalid Signature"
                };
            }

            DeleteIssue(con, req.AccessionNo, college, idNo);

            return new ReceiveBookResponseDto
            {
                Success = true,
                Mode = "receive",
                Message = "Book received successfully"
            };
        }

        // =========================================

        private async Task<UserExtraDetailDto>
            GetStudentStaffDetail(string idNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = "";

            if (idNo.Length == 6)
            {
                sql = @"SELECT Designation,Department
                        FROM Staff
                        WHERE IDNo=@IDNo";
            }
            else
            {
                sql = @"SELECT Course,Batch
                        FROM Admissions
                        WHERE IDNo=@IDNo";
            }

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                return new UserExtraDetailDto
                {
                    Course = idNo.Length == 6
                        ? dr["Designation"].ToString()
                        : dr["Course"].ToString(),

                    Batch = idNo.Length == 6
                        ? dr["Department"].ToString()
                        : dr["Batch"].ToString()
                };
            }

            return new UserExtraDetailDto();
        }

        // =========================================

        private async Task<string> GetPhoto(string idNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = idNo.Length == 10
                ? "SELECT Snap FROM Admissions WHERE IDNO=@IDNO"
                : "SELECT Snap FROM Staff WHERE IDNO=@IDNO";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNO", idNo);

            using SqlDataReader dr =
                await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync() && !dr.IsDBNull(0))
            {
                byte[] imageBytes = (byte[])dr[0];

                return "data:image/jpeg;base64,"
                    + Convert.ToBase64String(imageBytes);
            }

            return "";
        }

        // =========================================

        private async Task<bool> CheckSignature(
            string signature)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql =
                "SELECT COUNT(*) FROM UserMaster WHERE Password=@Password";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Password", signature);

            int count =
                (int)await cmd.ExecuteScalarAsync();

            return count > 0;
        }

        // =========================================

        private void DeleteIssue(
            SqlConnection con,
            long accNo,
            string college,
            long idNo)
        {
            string sql = @"DELETE FROM IssueRegister
                           WHERE AccessionNo=@Acc
                           AND CollegeName=@Col
                           AND IDNo=@ID";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@Acc", accNo);
            cmd.Parameters.AddWithValue("@Col", college);
            cmd.Parameters.AddWithValue("@ID", idNo);

            cmd.ExecuteNonQuery();
        }
    }
}

