using ClosedXML.Excel;
using lib.DtoModel.SearchISBNDto;
using lib.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace lib.Service
{
    public class SearchISBNService : BaseService, ISearchISBNService
    {
        private readonly string _connectionString;

        public SearchISBNService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // ==========================================
        // Search ISBN
        // ==========================================
        public async Task<List<ISBNSearchDto>> SearchAsync(string collegeName, string isbn)
        {
            List<ISBNSearchDto> books = new List<ISBNSearchDto>();

            using SqlConnection con = new SqlConnection(_connectionString);

            string query = @"
                SELECT 
                    Title,
                    AccessionNo,
                    Author,
                    Publisher,
                    Edition,
                    ISBN,
                    ClassNo,
                    BookNo,
                    Year,
                    Pages,
                    Price,
                    NetPrice,
                    Location 
                FROM StockRegister 
                WHERE CollegeName = @CollegeName 
                AND ISBN LIKE '%' + @ISBN + '%'";

            using SqlCommand cmd = new SqlCommand(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@ISBN", isbn ?? string.Empty);

            await con.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                books.Add(new ISBNSearchDto
                {
                    Title = reader["Title"]?.ToString(),
                    AccessionNo = reader["AccessionNo"]?.ToString(),
                    Author = reader["Author"]?.ToString(),
                    Publisher = reader["Publisher"]?.ToString(),
                    Edition = reader["Edition"]?.ToString(),
                    ISBN = reader["ISBN"]?.ToString(),
                    ClassNo = reader["ClassNo"]?.ToString(),
                    BookNo = reader["BookNo"]?.ToString(),
                    Year = reader["Year"]?.ToString(),
                    Pages = reader["Pages"]?.ToString(),
                    Price = reader["Price"]?.ToString(),
                    NetPrice = reader["NetPrice"] == DBNull.Value ? null : (double?)reader["NetPrice"],
                    Location = reader["Location"]?.ToString()
                });
            }

            return books;
        }

        // ==========================================
        // Export to Excel using ClosedXML
        // ==========================================
        public async Task<byte[]> ExportExcelAsync(string collegeName, string isbn)
        {
            var result = await SearchAsync(collegeName, isbn);

            using XLWorkbook workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("ISBN Gaps");

            // Header Row (13 columns)
            sheet.Cell(1, 1).Value = "Title";
            sheet.Cell(1, 2).Value = "Accession No.";
            sheet.Cell(1, 3).Value = "Author";
            sheet.Cell(1, 4).Value = "Publisher";
            sheet.Cell(1, 5).Value = "Edition";
            sheet.Cell(1, 6).Value = "ISBN";
            sheet.Cell(1, 7).Value = "Class No.";
            sheet.Cell(1, 8).Value = "Book No.";
            sheet.Cell(1, 9).Value = "Year";
            sheet.Cell(1, 10).Value = "Pages";
            sheet.Cell(1, 11).Value = "Price";
            sheet.Cell(1, 12).Value = "Net Price";
            sheet.Cell(1, 13).Value = "Location";

            // Headers Styling
            var headerRange = sheet.Range(1, 1, 1, 13);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
            headerRange.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var item in result)
            {
                sheet.Cell(row, 1).SetValue(item.Title ?? "");
                sheet.Cell(row, 2).SetValue(item.AccessionNo ?? "");
                sheet.Cell(row, 3).SetValue(item.Author ?? "");
                sheet.Cell(row, 4).SetValue(item.Publisher ?? "");
                sheet.Cell(row, 5).SetValue(item.Edition ?? "");
                sheet.Cell(row, 6).SetValue(item.ISBN ?? "");
                sheet.Cell(row, 7).SetValue(item.ClassNo ?? "");
                sheet.Cell(row, 8).SetValue(item.BookNo ?? "");
                sheet.Cell(row, 9).SetValue(item.Year ?? "");
                sheet.Cell(row, 10).SetValue(item.Pages ?? "");
                sheet.Cell(row, 11).SetValue(item.Price ?? "");
                
                if (item.NetPrice.HasValue)
                    sheet.Cell(row, 12).SetValue(item.NetPrice.Value);
                else
                    sheet.Cell(row, 12).SetValue("");

                sheet.Cell(row, 13).SetValue(item.Location ?? "");
                row++;
            }

            sheet.Columns().AdjustToContents();

            using MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);

            return ms.ToArray();
        }
    }
}
