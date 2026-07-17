using ClosedXML.Excel;
using lib.DtoModel.SearchIssueDatesDto;
using lib.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace lib.Service
{
    public class SearchIssueDatesService : ISearchIssueDatesService
    {
        private readonly string _connectionString;

        public SearchIssueDatesService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // ===============================
        // Search Issue Dates
        // ===============================
        public async Task<IssueDatesSearchResponseDto> SearchIssueDatesAsync(IssueDatesSearchRequestDto request)
        {
            IssueDatesSearchResponseDto response = new IssueDatesSearchResponseDto();

            try
            {
                List<IssueDetailDto> list = new List<IssueDetailDto>();

                using SqlConnection con = new SqlConnection(_connectionString);

                string query = @"
                    SELECT 
                        IssueDate,
                        IDNo,
                        Title,
                        AccessionNo,
                        WhomIssued,
                        LastReturnDate as LastDate,
                        Condition,
                        Discipline,
                        Type,
                        Remarks 
                    FROM IssueRegister 
                    WHERE CollegeName = @CollegeName 
                    AND IssueDate >= @IssueDatefrom 
                    AND IssueDate <= @IssueDateto 
                    ORDER BY IssueDate";

                using SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CollegeName", request.CollegeName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IssueDatefrom", request.IssueDateFrom ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@IssueDateto", request.IssueDateTo ?? (object)DBNull.Value);

                await con.OpenAsync();

                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new IssueDetailDto
                    {
                        IssueDate = reader["IssueDate"] == DBNull.Value ? null : (DateTime?)reader["IssueDate"],
                        IDNo = reader["IDNo"]?.ToString(),
                        Title = reader["Title"]?.ToString(),
                        AccessionNo = reader["AccessionNo"]?.ToString(),
                        WhomIssued = reader["WhomIssued"]?.ToString(),
                        LastDate = reader["LastDate"] == DBNull.Value ? null : (DateTime?)reader["LastDate"],
                        Condition = reader["Condition"]?.ToString(),
                        Discipline = reader["Discipline"]?.ToString(),
                        Type = reader["Type"]?.ToString(),
                        Remarks = reader["Remarks"]?.ToString()
                    });
                }

                response.Success = list.Any();
                response.Message = list.Any() ? "Records Found" : "No Records Found";
                response.Data = list;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }

        // =============================
        // Export Excel
        // =============================
        public async Task<byte[]> ExportIssueDatesExcelAsync(IssueDatesSearchRequestDto request)
        {
            var result = await SearchIssueDatesAsync(request);

            using XLWorkbook workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add("Issue Register");

            // Header Row
            sheet.Cell(1, 1).Value = "Issue Date";
            sheet.Cell(1, 2).Value = "ID No";
            sheet.Cell(1, 3).Value = "Title";
            sheet.Cell(1, 4).Value = "Accession No.";
            sheet.Cell(1, 5).Value = "Whom Issued";
            sheet.Cell(1, 6).Value = "Last Return Date";
            sheet.Cell(1, 7).Value = "Condition";
            sheet.Cell(1, 8).Value = "Discipline";
            sheet.Cell(1, 9).Value = "Type";
            sheet.Cell(1, 10).Value = "Remarks";

            // Headers Styling
            var headerRange = sheet.Range(1, 1, 1, 10);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
            headerRange.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var item in result.Data)
            {
                sheet.Cell(row, 1).SetValue(item.IssueDate?.ToString("MM/dd/yyyy") ?? "");
                sheet.Cell(row, 2).SetValue(item.IDNo ?? "");
                sheet.Cell(row, 3).SetValue(item.Title ?? "");
                sheet.Cell(row, 4).SetValue(item.AccessionNo ?? "");
                sheet.Cell(row, 5).SetValue(item.WhomIssued ?? "");
                sheet.Cell(row, 6).SetValue(item.LastDate?.ToString("MM/dd/yyyy") ?? "");
                sheet.Cell(row, 7).SetValue(item.Condition ?? "");
                sheet.Cell(row, 8).SetValue(item.Discipline ?? "");
                sheet.Cell(row, 9).SetValue(item.Type ?? "");
                sheet.Cell(row, 10).SetValue(item.Remarks ?? "");
                row++;
            }

            sheet.Columns().AdjustToContents();

            using MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);

            return ms.ToArray();
        }
    }
}
