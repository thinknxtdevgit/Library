using lib.DtoModel.SearchAccessionDto;
using lib.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;

namespace lib.Service
{
    public class SearchAccessionService : BaseService, ISearchAccessionService
    {
        private readonly string _connectionString;

        public SearchAccessionService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // ==========================================
        // Get Publishers dropdown items
        // ==========================================
        public async Task<List<string>> GetPublishersAsync(string collegeName)
        {
            List<string> publishers = new List<string>();
            using SqlConnection con = new SqlConnection(_connectionString);
            string query = "SELECT DISTINCT Publisher FROM Publishers WHERE collegename = @CollegeName ORDER BY Publisher";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);

            await con.OpenAsync();
            using SqlDataReader dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                var val = dr.GetValue(0)?.ToString();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    publishers.Add(val);
                }
            }
            return publishers;
        }

        // ==========================================
        // Get Categories dropdown items
        // ==========================================
        public async Task<List<string>> GetCategoriesAsync(string collegeName)
        {
            List<string> categories = new List<string>();
            using SqlConnection con = new SqlConnection(_connectionString);
            string query = "SELECT DISTINCT Category FROM Categories WHERE collegename = @CollegeName ORDER BY Category";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);

            await con.OpenAsync();
            using SqlDataReader dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                var val = dr.GetValue(0)?.ToString();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    categories.Add(val);
                }
            }
            return categories;
        }

        // ==========================================
        // Get Sources dropdown items
        // ==========================================
        public async Task<List<string>> GetSourcesAsync(string collegeName)
        {
            List<string> sources = new List<string>();
            using SqlConnection con = new SqlConnection(_connectionString);
            string query = "SELECT DISTINCT Source FROM SourceBooks WHERE collegename = @CollegeName ORDER BY Source";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);

            await con.OpenAsync();
            using SqlDataReader dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                var val = dr.GetValue(0)?.ToString();
                if (!string.IsNullOrWhiteSpace(val))
                {
                    sources.Add(val);
                }
            }
            return sources;
        }

        // ==========================================
        // Search Accession details
        // ==========================================
        public async Task<AccessionSearchResponse?> SearchAccessionAsync(string collegeName, string accessionNo)
        {
            AccessionDetailDto? book = null;
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = "SELECT * FROM StockRegister WHERE CollegeName = @CollegeName AND AccessionNo = @AccessionNo";
                using SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo ?? (object)DBNull.Value);

                await con.OpenAsync();
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    book = new AccessionDetailDto
                    {
                        BindingBook = reader["BindingBook"]?.ToString(),
                        Title = reader["Title"]?.ToString(),
                        FirstAuthorForeName = reader["FirstAuthorForeName"]?.ToString(),
                        SecondAuthorForeName = reader["SecondAuthorForeName"]?.ToString(),
                        ThirdAuthorForeName = reader["ThirdAuthorForeName"]?.ToString(),
                        FirstAuthorSirName = reader["FirstAuthorSirName"]?.ToString(),
                        SecondAuthorSirName = reader["SecondAuthorSirName"]?.ToString(),
                        ThirdAuthorSirName = reader["ThirdAuthorSirName"]?.ToString(),
                        Author = reader["Author"]?.ToString(),
                        MoreThanThreeAuthors = reader["MoreThanThreeAuthors"]?.ToString(),
                        Publisher = reader["Publisher"]?.ToString(),
                        Edition = reader["Edition"]?.ToString(),
                        Price = reader["Price"]?.ToString(),
                        Discount = reader["Discount"]?.ToString(),
                        NetPrice = reader["NetPrice"]?.ToString(),
                        Year = reader["Year"]?.ToString(),
                        Pages = reader["Pages"]?.ToString(),
                        BillNo = reader["BillNo"]?.ToString(),
                        BillDate = reader["BillDate"]?.ToString(),
                        Location = reader["Location"]?.ToString(),
                        ClassNo = reader["ClassNo"]?.ToString(),
                        BookNo = reader["BookNo"]?.ToString(),
                        SubTitle = reader["SubTitle"]?.ToString(),
                        ISBN = reader["ISBN"]?.ToString(),
                        Place = reader["Place"]?.ToString(),
                        BookSize = reader["BookSize"]?.ToString(),
                        Series = reader["Series"]?.ToString(),
                        Subject1 = reader["Subject1"]?.ToString(),
                        Subject2 = reader["Subject2"]?.ToString(),
                        Remarks = reader["Remarks"]?.ToString(),
                        Type = reader["Type"]?.ToString(),
                        Category = reader["Category"]?.ToString(),
                        Source = reader["Source"]?.ToString(),
                        CollegeName = reader["CollegeName"]?.ToString(),
                        AccessionNo = reader["AccessionNo"]?.ToString()
                    };
                }
            }

            if (book == null)
            {
                return null;
            }

            AccessionSearchResponse response = new AccessionSearchResponse { Book = book };

            // Query IssueRegister
            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string query = @"
                    SELECT 
                        IssueDate, AccessionNo, IDNo, WhomIssued, Discipline, LastReturnDate, Remarks 
                    FROM IssueRegister 
                    WHERE CollegeName = @CollegeName 
                    AND AccessionNo = @AccessionNo";
                
                using SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo ?? (object)DBNull.Value);

                await con.OpenAsync();
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    response.Issue = new AccessionIssueDetailDto
                    {
                        IssueDate = reader["IssueDate"] == DBNull.Value ? null : (DateTime?)reader["IssueDate"],
                        AccessionNo = reader["AccessionNo"]?.ToString(),
                        IDNo = reader["IDNo"] == DBNull.Value ? null : (long?)Convert.ToInt64(reader["IDNo"]),
                        WhomIssued = reader["WhomIssued"]?.ToString(),
                        Discipline = reader["Discipline"]?.ToString(),
                        LastReturnDate = reader["LastReturnDate"] == DBNull.Value ? null : (DateTime?)reader["LastReturnDate"],
                        Remarks = reader["Remarks"]?.ToString()
                    };
                }
            }

            // Query ClassRollNo and UniRollNo if IDNo exists
            if (response.Issue?.IDNo != null)
            {
                using (SqlConnection con = new SqlConnection(_connectionString))
                {
                    string query = "SELECT UniRollNo, ClassRollNo FROM Admissions WHERE IDNo = @IDNo AND CollegeName = @CollegeName";
                    using SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@IDNo", response.Issue.IDNo.Value);
                    cmd.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);

                    await con.OpenAsync();
                    using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                    if (await reader.ReadAsync())
                    {
                        response.Issue.ClassRollNo = reader["ClassRollNo"]?.ToString();
                        response.Issue.UniRollNo = reader["UniRollNo"]?.ToString();
                    }
                }
            }

            return response;
        }

        // ==========================================
        // Get Student Photo varbinary Snap
        // ==========================================
        public async Task<byte[]?> GetStudentImageAsync(string collegeName, long idNo)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            string query = "SELECT Snap FROM Admissions WHERE IDNo = @IDNo AND CollegeName = @CollegeName";
            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@IDNo", idNo);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);

            await con.OpenAsync();
            var result = await cmd.ExecuteScalarAsync();
            if (result != null && result != DBNull.Value)
            {
                return (byte[])result;
            }
            return null;
        }

        // ==========================================
        // Update Stock Register details
        // ==========================================
        public async Task<bool> UpdateStockAsync(AccessionUpdateRequest request)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            string query = @"
                UPDATE StockRegister 
                SET 
                    BindingBook = @BindingBook,
                    Author = @Author,
                    FirstAuthorForeName = @FirstAuthorForeName,
                    FirstAuthorSirName = @FirstAuthorSirName,
                    SecondAuthorForeName = @SecondAuthorForeName,
                    SecondAuthorSirName = @SecondAuthorSirName,
                    ThirdAuthorForeName = @ThirdAuthorForeName,
                    ThirdAuthorSirName = @ThirdAuthorSirName,
                    MoreThanThreeAuthors = @MoreThanThreeAuthors,
                    Publisher = @Publisher,
                    Edition = @Edition,
                    Price = @Price,
                    Discount = @Discount,
                    NetPrice = @NetPrice,
                    Type = @Type,
                    Category = @Category,
                    Source = @Source,
                    Year = @Year,
                    Pages = @Pages,
                    BillNo = @BillNo,
                    BillDate = @BillDate,
                    Location = @Location,
                    ClassNo = @ClassNo,
                    BookNo = @BookNo,
                    SubTitle = @SubTitle,
                    ISBN = @ISBN,
                    Place = @Place,
                    BookSize = @BookSize,
                    Series = @Series,
                    Subject1 = @Subject1,
                    Subject2 = @Subject2,
                    Remarks = @Remarks 
                WHERE CollegeName = @CollegeName 
                AND AccessionNo = @AccessionNo";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@BindingBook", request.BindingBook ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CollegeName", request.CollegeName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AccessionNo", request.AccessionNo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@MoreThanThreeAuthors", request.MoreThanThreeAuthors ?? "False");
            
            cmd.Parameters.AddWithValue("@Author", string.IsNullOrWhiteSpace(request.Author) ? "None" : request.Author);
            cmd.Parameters.AddWithValue("@FirstAuthorForeName", string.IsNullOrWhiteSpace(request.FirstAuthorForeName) ? DBNull.Value : request.FirstAuthorForeName);
            cmd.Parameters.AddWithValue("@FirstAuthorSirName", string.IsNullOrWhiteSpace(request.FirstAuthorSirName) ? DBNull.Value : request.FirstAuthorSirName);
            cmd.Parameters.AddWithValue("@SecondAuthorForeName", string.IsNullOrWhiteSpace(request.SecondAuthorForeName) ? DBNull.Value : request.SecondAuthorForeName);
            cmd.Parameters.AddWithValue("@SecondAuthorSirName", string.IsNullOrWhiteSpace(request.SecondAuthorSirName) ? DBNull.Value : request.SecondAuthorSirName);
            cmd.Parameters.AddWithValue("@ThirdAuthorForeName", string.IsNullOrWhiteSpace(request.ThirdAuthorForeName) ? DBNull.Value : request.ThirdAuthorForeName);
            cmd.Parameters.AddWithValue("@ThirdAuthorSirName", string.IsNullOrWhiteSpace(request.ThirdAuthorSirName) ? DBNull.Value : request.ThirdAuthorSirName);

            cmd.Parameters.AddWithValue("@Publisher", string.IsNullOrWhiteSpace(request.Publisher) || request.Publisher == "Select" ? DBNull.Value : request.Publisher);
            cmd.Parameters.AddWithValue("@Edition", string.IsNullOrWhiteSpace(request.Edition) ? DBNull.Value : request.Edition);
            cmd.Parameters.AddWithValue("@Price", string.IsNullOrWhiteSpace(request.Price) ? DBNull.Value : request.Price);
            cmd.Parameters.AddWithValue("@Discount", string.IsNullOrWhiteSpace(request.Discount) ? DBNull.Value : request.Discount);
            cmd.Parameters.AddWithValue("@NetPrice", string.IsNullOrWhiteSpace(request.NetPrice) ? DBNull.Value : request.NetPrice);
            cmd.Parameters.AddWithValue("@Type", string.IsNullOrWhiteSpace(request.Type) || request.Type == "Select" ? DBNull.Value : request.Type);
            cmd.Parameters.AddWithValue("@Category", string.IsNullOrWhiteSpace(request.Category) || request.Category == "Select" ? "None" : request.Category);
            cmd.Parameters.AddWithValue("@Source", string.IsNullOrWhiteSpace(request.Source) || request.Source == "Select" ? DBNull.Value : request.Source);

            cmd.Parameters.AddWithValue("@Year", string.IsNullOrWhiteSpace(request.Year) ? DBNull.Value : request.Year);
            cmd.Parameters.AddWithValue("@Pages", string.IsNullOrWhiteSpace(request.Pages) ? DBNull.Value : request.Pages);
            cmd.Parameters.AddWithValue("@BillNo", string.IsNullOrWhiteSpace(request.BillNo) ? DBNull.Value : request.BillNo);
            cmd.Parameters.AddWithValue("@BillDate", string.IsNullOrWhiteSpace(request.BillDate) ? DBNull.Value : request.BillDate);
            cmd.Parameters.AddWithValue("@Location", string.IsNullOrWhiteSpace(request.Location) ? DBNull.Value : request.Location);
            cmd.Parameters.AddWithValue("@ClassNo", string.IsNullOrWhiteSpace(request.ClassNo) ? DBNull.Value : request.ClassNo);
            cmd.Parameters.AddWithValue("@BookNo", string.IsNullOrWhiteSpace(request.BookNo) ? DBNull.Value : request.BookNo);
            cmd.Parameters.AddWithValue("@SubTitle", string.IsNullOrWhiteSpace(request.SubTitle) ? DBNull.Value : request.SubTitle);
            cmd.Parameters.AddWithValue("@ISBN", string.IsNullOrWhiteSpace(request.ISBN) ? DBNull.Value : request.ISBN);
            cmd.Parameters.AddWithValue("@Place", string.IsNullOrWhiteSpace(request.Place) ? DBNull.Value : request.Place);
            cmd.Parameters.AddWithValue("@BookSize", string.IsNullOrWhiteSpace(request.BookSize) ? DBNull.Value : request.BookSize);
            cmd.Parameters.AddWithValue("@Series", string.IsNullOrWhiteSpace(request.Series) ? DBNull.Value : request.Series);
            cmd.Parameters.AddWithValue("@Subject1", string.IsNullOrWhiteSpace(request.Subject1) ? DBNull.Value : request.Subject1);
            cmd.Parameters.AddWithValue("@Subject2", string.IsNullOrWhiteSpace(request.Subject2) ? DBNull.Value : request.Subject2);
            cmd.Parameters.AddWithValue("@Remarks", string.IsNullOrWhiteSpace(request.Remarks) ? DBNull.Value : request.Remarks);

            await con.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }

        // ==========================================
        // Update Issue Register remarks
        // ==========================================
        public async Task<bool> UpdateIssueRemarksAsync(string collegeName, string accessionNo, long idNo, string remarks)
        {
            using SqlConnection con = new SqlConnection(_connectionString);
            string query = @"
                UPDATE IssueRegister 
                SET Remarks = @Remarks 
                WHERE CollegeName = @CollegeName 
                AND AccessionNo = @AccessionNo 
                AND IDNo = @IDNo";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@Remarks", remarks ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@AccessionNo", accessionNo ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@IDNo", idNo);

            await con.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();
            return rows > 0;
        }
    }
}
