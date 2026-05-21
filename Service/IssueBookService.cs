using lib.DtoModel.IssueBook;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class IssueBookService: IIssueBookService
    {
        private readonly string _connectionString;

        public IssueBookService(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection");
        }

        private int StudBookIssueDays = 7;
        private int StaffBookIssueDays = 15;

        public async Task<IssueBookResponseDto> CheckIdAsync(
            IssueBookRequestDto request)
        {
            if (request == null || string.IsNullOrEmpty(request.txtidno))
            {
                return new IssueBookResponseDto
                {
                    Success = false,
                    Message = "ID cannot be empty"
                };
            }

            bool isStudent = request.txtidno.Length == 10;
            bool isStaff = request.txtidno.Length == 6;

            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            if (isStudent)
            {
                return await GetStudentDetail(
                    con,
                    request.txtidno,
                    request.txtaccessionno);
            }

            if (isStaff)
            {
                return await GetStaffDetail(
                    con,
                    request.txtidno,
                    request.txtaccessionno);
            }

            return new IssueBookResponseDto
            {
                Success = false,
                Message = "Invalid ID Format"
            };
        }

        // ================= STUDENT =================

        private async Task<IssueBookResponseDto> GetStudentDetail(
            SqlConnection con,
            string idNo,
            string accessionNo)
        {
            string sql = @"SELECT CollegeName,Snap,StudentName,
                           Course,Batch,uniRollNo
                           FROM Admissions
                           WHERE IDNO=@IDNO";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNO", idNo);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            if (!dr.Read())
            {
                return new IssueBookResponseDto
                {
                    Success = false,
                    Message = "Invalid Student ID"
                };
            }

            var collegeName = dr["CollegeName"].ToString();

            UserDetailDto user = new UserDetailDto
            {
                CollegeName = collegeName,
                Name = dr["StudentName"].ToString(),
                Course = dr["Course"].ToString(),
                Batch = dr["Batch"].ToString(),
                UnivRollNo = dr["uniRollNo"].ToString(),
                Type = "Student",
                LastReturnDate =
                    DateTime.Today.AddDays(StudBookIssueDays),

                Image = dr["Snap"] != DBNull.Value
                    ? "data:image/jpeg;base64," +
                      Convert.ToBase64String((byte[])dr["Snap"])
                    : null
            };

            dr.Close();

            var previous =
                await GetPreviousIssues(idNo, collegeName);

            int totalBooks =
                await GetIssuedBookCount(idNo, collegeName);

            BookDetailDto book = null;

            if (!string.IsNullOrEmpty(accessionNo))
            {
                book = await GetBookDetail(accessionNo);
            }

            return new IssueBookResponseDto
            {
                Success = true,
                UserDetail = user,
                PreviousIssues = previous,
                TotalIssuedBooks = totalBooks,
                BookDetail = book
            };
        }

        // ================= STAFF =================

        private async Task<IssueBookResponseDto> GetStaffDetail(
            SqlConnection con,
            string idNo,
            string accessionNo)
        {
            string sql = @"SELECT CollegeName,Snap,Name,
                           Designation,Department,idno
                           FROM Staff
                           WHERE IDNO=@IDNO";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNO", idNo);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            if (!dr.Read())
            {
                return new IssueBookResponseDto
                {
                    Success = false,
                    Message = "Invalid Staff ID"
                };
            }

            var collegeName = dr["CollegeName"].ToString();

            UserDetailDto user = new UserDetailDto
            {
                CollegeName = collegeName,
                Name = dr["Name"].ToString(),
                Designation = dr["Designation"].ToString(),
                Department = dr["Department"].ToString(),
                Type = "Staff",
                LastReturnDate =
                    DateTime.Today.AddDays(StaffBookIssueDays),

                Image = dr["Snap"] != DBNull.Value
                    ? "data:image/jpeg;base64," +
                      Convert.ToBase64String((byte[])dr["Snap"])
                    : null
            };

            dr.Close();

            var previous =
                await GetPreviousIssues(idNo, collegeName);

            int totalBooks =
                await GetIssuedBookCount(idNo, collegeName);

            BookDetailDto book = null;

            if (!string.IsNullOrEmpty(accessionNo))
            {
                book = await GetBookDetail(accessionNo);
            }

            return new IssueBookResponseDto
            {
                Success = true,
                UserDetail = user,
                PreviousIssues = previous,
                TotalIssuedBooks = totalBooks,
                BookDetail = book
            };
        }

        // ================= BOOK DETAIL =================

        private async Task<BookDetailDto> GetBookDetail(
            string accessionNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"SELECT Title,Author,Category,
                           AccessionNo
                           FROM StockRegister
                           WHERE AccessionNo=@AccessionNo";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            if (dr.Read())
            {
                return new BookDetailDto
                {
                    Success = true,
                    AccessionNo =
                        dr["AccessionNo"].ToString(),

                    Title = dr["Title"].ToString(),

                    Author = dr["Author"].ToString(),

                    Category = dr["Category"].ToString()
                };
            }

            return new BookDetailDto
            {
                Success = false,
                Message = "Book not found"
            };
        }

        // ================= PREVIOUS ISSUE =================

        private async Task<List<PreviousIssueDto>>
            GetPreviousIssues(string idNo, string collegeName)
        {
            List<PreviousIssueDto> list =
                new List<PreviousIssueDto>();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"SELECT Title,Author,AccessionNo,
                           IssueDate,LastReturnDate,
                           IDNo,Discipline
                           FROM IssueRegister
                           WHERE IDNo=@IDNo
                           AND CollegeName=@CollegeName";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (dr.Read())
            {
                list.Add(new PreviousIssueDto
                {
                    Title = dr["Title"].ToString(),

                    Author = dr["Author"].ToString(),

                    AccessionNo =
                        dr["AccessionNo"].ToString(),

                    IssueDate =
                        Convert.ToDateTime(dr["IssueDate"])
                        .ToString("dd/MM/yyyy"),

                    LastReturnDate =
                        Convert.ToDateTime(
                            dr["LastReturnDate"])
                        .ToString("dd/MM/yyyy"),

                    IDNo = dr["IDNo"].ToString(),

                    Course = dr["Discipline"].ToString()
                });
            }

            return list;
        }

        // ================= TOTAL BOOKS =================

        private async Task<int> GetIssuedBookCount(
            string idNo,
            string collegeName)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string sql = @"SELECT COUNT(*)
                           FROM IssueRegister
                           WHERE IDNo=@IDNo
                           AND CollegeName=@CollegeName";

            using SqlCommand cmd = new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            cmd.Parameters.AddWithValue(
                "@CollegeName",
                collegeName);

            return (int)await cmd.ExecuteScalarAsync();
        }
    }
}

