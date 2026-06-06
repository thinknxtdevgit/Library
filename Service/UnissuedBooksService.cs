using lib.DtoModel.UnissuedBookDto;
using lib.Interface;
using lib.Pagination_Helper;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class UnissuedBooksService : IUnissuedBooksService
    {
        private readonly string _connectionString;
        private readonly DbPaginationHelper _paginationHelper;
        public UnissuedBooksService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _paginationHelper = new DbPaginationHelper(_connectionString);
       
        }

        public async Task<List<UnissuedBookDto>> GetUnissuedBooksAsync(string collegeName)
        {
            var list = new List<UnissuedBookDto>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                string sql = @"
                SELECT sr.DateEntry, sr.AccessionNo, sr.Title, sr.Author,
                       sr.Edition, sr.Source, sr.Publisher, sr.ClassNo, sr.BookNo
                FROM StockRegister sr
                WHERE sr.CollegeName = @CollegeName
                AND sr.AccessionNo NOT IN 
                (
                    SELECT ir.AccessionNo 
                    FROM IssueRegister ir 
                    WHERE ir.CollegeName = @CollegeName
                )";

                using (SqlCommand cmd = new SqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@CollegeName", collegeName);

                    await con.OpenAsync();

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            list.Add(new UnissuedBookDto
                            {
                                DateEntry = reader["DateEntry"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["DateEntry"]),
                                AccessionNo = reader["AccessionNo"].ToString(),
                                Title = reader["Title"].ToString(),
                                Author = reader["Author"].ToString(),
                                Edition = reader["Edition"].ToString(),
                                Source = reader["Source"].ToString(),
                                Publisher = reader["Publisher"].ToString(),
                                ClassNo = reader["ClassNo"].ToString(),
                                BookNo = reader["BookNo"].ToString()
                            });
                        }
                    }
                }
            }

            return list;
        }

        public async Task<PagedResult<UnissuedBookDto>> GetUnissuedBooksAsyncPages(string collegeName, int pageNumber, int pageSize)

        {
            string countQuery = @"
    SELECT COUNT(*)
    FROM StockRegister sr
    WHERE sr.CollegeName = @CollegeName
    AND sr.AccessionNo NOT IN
    (
        SELECT ir.AccessionNo
        FROM IssueRegister ir
        WHERE ir.CollegeName = @CollegeName
    )";

            string dataQuery = @"
    SELECT
        sr.DateEntry,
        sr.AccessionNo,
        sr.Title,
        sr.Author,
        sr.Edition,
        sr.Source,
        sr.Publisher,
        sr.ClassNo,
        sr.BookNo
    FROM StockRegister sr
    WHERE sr.CollegeName = @CollegeName
    AND sr.AccessionNo NOT IN
    (
        SELECT ir.AccessionNo
        FROM IssueRegister ir
        WHERE ir.CollegeName = @CollegeName
    )
    ORDER BY sr.AccessionNo
    OFFSET @Offset ROWS
    FETCH NEXT @PageSize ROWS ONLY";

            SqlParameter[] parameters =
            {
              new SqlParameter("@CollegeName", collegeName)
            };

            return await _paginationHelper.GetPagedResultAsync(
                dataQuery,
                countQuery,
                parameters,
                pageNumber,
                pageSize,
                reader => new UnissuedBookDto
                {
                    DateEntry =
                        reader["DateEntry"] == DBNull.Value
                        ? DateTime.MinValue
                        : Convert.ToDateTime(reader["DateEntry"]),

                    AccessionNo =
                        reader["AccessionNo"]?.ToString(),

                    Title =
                        reader["Title"]?.ToString(),

                    Author =
                        reader["Author"]?.ToString(),

                    Edition =
                        reader["Edition"]?.ToString(),

                    Source =
                        reader["Source"]?.ToString(),

                    Publisher =
                        reader["Publisher"]?.ToString(),

                    ClassNo =
                        reader["ClassNo"]?.ToString(),

                    BookNo =
                        reader["BookNo"]?.ToString()
                });
        }
        //public byte[] ExportToExcel(List<UnissuedBookDto> data) 
        //{
        //    using (var package = new OfficeOpenXml.ExcelPackage())
        //    {
        //        var ws = package.Workbook.Worksheets.Add("UnissuedBooks");

        //        // Header
        //        ws.Cells[1, 1].Value = "Date Entry";
        //        ws.Cells[1, 2].Value = "Accession No";
        //        ws.Cells[1, 3].Value = "Title";
        //        ws.Cells[1, 4].Value = "Author";
        //        ws.Cells[1, 5].Value = "Edition";
        //        ws.Cells[1, 6].Value = "Source";
        //        ws.Cells[1, 7].Value = "Publisher";
        //        ws.Cells[1, 8].Value = "Class No";
        //        ws.Cells[1, 9].Value = "Book No";

        //        int row = 2;

        //        foreach (var item in data)
        //        {
        //            ws.Cells[row, 1].Value = item.DateEntry;
        //            ws.Cells[row, 2].Value = item.AccessionNo;
        //            ws.Cells[row, 3].Value = item.Title;
        //            ws.Cells[row, 4].Value = item.Author;
        //            ws.Cells[row, 5].Value = item.Edition;
        //            ws.Cells[row, 6].Value = item.Source;
        //            ws.Cells[row, 7].Value = item.Publisher;
        //            ws.Cells[row, 8].Value = item.ClassNo;
        //            ws.Cells[row, 9].Value = item.BookNo;
        //            row++;
        //        }

        //        return package.GetAsByteArray();
        //    }
        //  }
    }

}
