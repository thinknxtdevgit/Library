using ClosedXML.Excel;
using lib.DtoModel.SearchStudentNameDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class SearchStudentNameService: ISearchStudentNameService
    {
        private readonly string _connectionString;
        public SearchStudentNameService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        // ===============================
        // Load Colleges
        // ===============================
        public async Task<List<string>> GetCollegesAsync()
        {
            List<string> colleges = new();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            string query = @"
                SELECT DISTINCT CollegeName
                FROM Admissions
                ORDER BY CollegeName";

            using SqlCommand cmd =
                new SqlCommand(query, con);

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                colleges.Add(
                    reader["CollegeName"].ToString() ?? "");
            }

            return colleges;
        }

        // ===============================
        // Search Student
        // ===============================
        public async Task<StudentSearchResponseDto> SearchStudentAsync(
            string collegeName,
            string studentName)
        {
            StudentSearchResponseDto response =
                new StudentSearchResponseDto();

            try
            {
                List<StudentDetailDto> students = new();

                using SqlConnection con =
                    new SqlConnection(_connectionString);

                string query = @"
                    SELECT
                        StudentName,
                        IDNo,
                        Course,
                        ClassRollNo,
                        FatherName,
                        PermanentAddress,
                        PhoneNo
                    FROM Admissions
                    WHERE CollegeName = @CollegeName
                    AND StudentName LIKE '%' + @StudentName + '%'
                    ORDER BY StudentName";

                using SqlCommand cmd =
                    new SqlCommand(query, con);

                cmd.Parameters.AddWithValue(
                    "@CollegeName",
                    collegeName);

                cmd.Parameters.AddWithValue(
                    "@StudentName",
                    studentName ?? "");

                await con.OpenAsync();

                using SqlDataReader reader =
                    await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    students.Add(new StudentDetailDto
                    {
                        StudentName =
                            reader["StudentName"]?.ToString(),

                        IDNo =
                            reader["IDNo"]?.ToString(),

                        Course =
                            reader["Course"]?.ToString(),

                        ClassRollNo =
                            reader["ClassRollNo"]?.ToString(),

                        FatherName =
                            reader["FatherName"]?.ToString(),

                        PermanentAddress =
                            reader["PermanentAddress"]?.ToString(),

                        PhoneNo =
                            reader["PhoneNo"]?.ToString()
                    });
                }

                response.Success = students.Any();

                response.Message = students.Any()
                    ? "Records Found"
                    : "No Records Found";

                response.Data = students;
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
        public async Task<byte[]> ExportStudentExcelAsync(
            string collegeName,
            string studentName)
        {
            var result =
                await SearchStudentAsync(
                    collegeName,
                    studentName);

            using XLWorkbook workbook =
                new XLWorkbook();

            var sheet =
                workbook.Worksheets.Add("Students");

            sheet.Cell(1, 1).Value = "Student Name";
            sheet.Cell(1, 2).Value = "ID No";
            sheet.Cell(1, 3).Value = "Course";
            sheet.Cell(1, 4).Value = "Class Roll No";
            sheet.Cell(1, 5).Value = "Father Name";
            sheet.Cell(1, 6).Value = "Permanent Address";
            sheet.Cell(1, 7).Value = "Phone No";

            int row = 2;

            foreach (var item in result.Data)
            {
                sheet.Cell(row, 1).Value = item.StudentName;
                sheet.Cell(row, 2).Value = item.IDNo;
                sheet.Cell(row, 3).Value = item.Course;
                sheet.Cell(row, 4).Value = item.ClassRollNo;
                sheet.Cell(row, 5).Value = item.FatherName;
                sheet.Cell(row, 6).Value = item.PermanentAddress;
                sheet.Cell(row, 7).Value = item.PhoneNo;

                row++;
            }

            sheet.Columns().AdjustToContents();

            using MemoryStream ms =
                new MemoryStream();

            workbook.SaveAs(ms);

            return ms.ToArray();
        }
    }

}



