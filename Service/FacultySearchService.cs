using ClosedXML.Excel;
using lib.DtoModel.FacultySearchDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class FacultySearchService : IFacultySearchService
    {
        private readonly string _connectionString;

        public FacultySearchService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // ===============================
        // Search Faculty
        // ===============================
        public async Task<FacultySearchResponseDto> SearchFacultyAsync(FacultySearchRequestDto request)
        {
            FacultySearchResponseDto response = new FacultySearchResponseDto();

            try
            {
                List<FacultyDetailDto> faculties = new();

                using SqlConnection con = new SqlConnection(_connectionString);

                string tableName = request.IsLeft ? "EmployeeLeft" : "Staff";
                string query = $@"
                    SELECT 
                        IDNo, 
                        Name, 
                        Designation, 
                        FatherName, 
                        MobileNo, 
                        PermanentAddress 
                    FROM {tableName}
                    WHERE CollegeName = @CollegeName 
                    AND Name LIKE '%' + @FacultyName + '%'
                    ORDER BY Name";

                using SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@CollegeName", request.CollegeName ?? (object)DBNull.Value);
                cmd.Parameters.AddWithValue("@FacultyName", request.FacultyName ?? string.Empty);

                await con.OpenAsync();

                using SqlDataReader reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    faculties.Add(new FacultyDetailDto
                    {
                        IDNo = reader["IDNo"]?.ToString(),
                        Name = reader["Name"]?.ToString(),
                        Designation = reader["Designation"]?.ToString(),
                        FatherName = reader["FatherName"]?.ToString(),
                        MobileNo = reader["MobileNo"]?.ToString(),
                        PermanentAddress = reader["PermanentAddress"]?.ToString()
                    });
                }

                response.Success = faculties.Any();
                response.Message = faculties.Any() ? "Records Found" : "No Records Found";
                response.Data = faculties;
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
        public async Task<byte[]> ExportFacultyExcelAsync(FacultySearchRequestDto request)
        {
            var result = await SearchFacultyAsync(request);

            using XLWorkbook workbook = new XLWorkbook();
            var sheet = workbook.Worksheets.Add(request.IsLeft ? "Left Employees" : "Staff");

            // Header Row
            sheet.Cell(1, 1).Value = "ID No";
            sheet.Cell(1, 2).Value = "Name";
            sheet.Cell(1, 3).Value = "Designation";
            sheet.Cell(1, 4).Value = "Father Name";
            sheet.Cell(1, 5).Value = "Mobile No.";
            sheet.Cell(1, 6).Value = "Permanent Address";

            // Headers Styling
            var headerRange = sheet.Range(1, 1, 1, 6);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
            headerRange.Style.Font.FontColor = XLColor.White;

            int row = 2;
            foreach (var item in result.Data)
            {
                sheet.Cell(row, 1).SetValue(item.IDNo ?? "");
                sheet.Cell(row, 2).SetValue(item.Name ?? "");
                sheet.Cell(row, 3).SetValue(item.Designation ?? "");
                sheet.Cell(row, 4).SetValue(item.FatherName ?? "");
                sheet.Cell(row, 5).SetValue(item.MobileNo ?? "");
                sheet.Cell(row, 6).SetValue(item.PermanentAddress ?? "");
                row++;
            }

            sheet.Columns().AdjustToContents();

            using MemoryStream ms = new MemoryStream();
            workbook.SaveAs(ms);

            return ms.ToArray();
        }
    }
}
