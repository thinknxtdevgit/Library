using ClosedXML.Excel;
using lib.DtoModel.TeacherSettingDto;
using lib.Interface;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Service
{
    public class TeacherSettingService: ITeacherSettingService
    {
        private readonly IConfiguration _configuration;

        public TeacherSettingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        public async Task<List<TeacherSettingDto>> GetTeachers(string collegeName)
        {
            List<TeacherSettingDto> teachers = new();

            using SqlConnection con = GetConnection();

            string query = @"SELECT CollegeName,
                                    IDNo,
                                    Name,
                                    Designation,
                                    FatherName,
                                    PermanentAddress,
                                    ContactNo,
                                    MobileNo,
                                    EMailID
                             FROM Staff
                             WHERE CollegeName=@CollegeName
                             ORDER BY IDNo";

            using SqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            await con.OpenAsync();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                teachers.Add(new TeacherSettingDto
                {
                    CollegeName = reader["CollegeName"]?.ToString(),
                    IDNo = Convert.ToInt64(reader["IDNo"]),
                    Name = reader["Name"]?.ToString(),
                    Designation = reader["Designation"]?.ToString(),
                    FatherName = reader["FatherName"]?.ToString(),
                    PermanentAddress = reader["PermanentAddress"]?.ToString(),
                    ContactNo = reader["ContactNo"]?.ToString(),
                    MobileNo = reader["MobileNo"]?.ToString(),
                    EMailID = reader["EMailID"]?.ToString()
                });
            }

            return teachers;
        }

        public async Task<int> GetTotalTeachers(string collegeName)
        {
            using SqlConnection con = GetConnection();

            string query = @"SELECT COUNT(*)
                             FROM Staff
                             WHERE CollegeName=@CollegeName";

            using SqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            await con.OpenAsync();

            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }

        public async Task<bool> AddTeacher(TeacherSettingDto dto)
        {
            using SqlConnection con = GetConnection();

            string query = @"INSERT INTO Staff
                            (
                                CollegeName,
                                IDNo,
                                Name,
                                Designation,
                                FatherName,
                                PermanentAddress,
                                ContactNo,
                                MobileNo,
                                EMailID
                            )
                            VALUES
                            (
                                @CollegeName,
                                @IDNo,
                                @Name,
                                @Designation,
                                @FatherName,
                                @PermanentAddress,
                                @ContactNo,
                                @MobileNo,
                                @EMailID
                            )";

            using SqlCommand cmd = new(query, con);

            cmd.Parameters.AddWithValue("@CollegeName", dto.CollegeName ?? "");
            cmd.Parameters.AddWithValue("@IDNo", dto.IDNo);
            cmd.Parameters.AddWithValue("@Name", dto.Name ?? "");
            cmd.Parameters.AddWithValue("@Designation", dto.Designation ?? "");
            cmd.Parameters.AddWithValue("@FatherName", dto.FatherName ?? "");
            cmd.Parameters.AddWithValue("@PermanentAddress", dto.PermanentAddress ?? "");
            cmd.Parameters.AddWithValue("@ContactNo", dto.ContactNo ?? "");
            cmd.Parameters.AddWithValue("@MobileNo", dto.MobileNo ?? "");
            cmd.Parameters.AddWithValue("@EMailID", dto.EMailID ?? "");

            await con.OpenAsync();

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateTeacher(string oldId, TeacherSettingDto dto)
        {
            using SqlConnection con = GetConnection();

            string query = @"UPDATE Staff
                             SET
                                CollegeName=@CollegeName,
                                IDNo=@IDNo,
                                Name=@Name,
                                Designation=@Designation,
                                FatherName=@FatherName,
                                PermanentAddress=@PermanentAddress,
                                ContactNo=@ContactNo,
                                MobileNo=@MobileNo,
                                EMailID=@EMailID
                             WHERE CollegeName=@CollegeName
                             AND IDNo=@OldID";

            using SqlCommand cmd = new(query, con);

            cmd.Parameters.AddWithValue("@CollegeName", dto.CollegeName ?? "");
            cmd.Parameters.AddWithValue("@IDNo", dto.IDNo);
            cmd.Parameters.AddWithValue("@Name", dto.Name ?? "");
            cmd.Parameters.AddWithValue("@Designation", dto.Designation ?? "");
            cmd.Parameters.AddWithValue("@FatherName", dto.FatherName ?? "");
            cmd.Parameters.AddWithValue("@PermanentAddress", dto.PermanentAddress ?? "");
            cmd.Parameters.AddWithValue("@ContactNo", dto.ContactNo ?? "");
            cmd.Parameters.AddWithValue("@MobileNo", dto.MobileNo ?? "");
            cmd.Parameters.AddWithValue("@EMailID", dto.EMailID ?? "");
            cmd.Parameters.AddWithValue("@OldID", oldId);

            await con.OpenAsync();

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<byte[]> ExportExcelAsync(string collegeName)
        {
            using SqlConnection con = GetConnection();

            string query = @"SELECT
                                CollegeName,
                                IDNo,
                                Name,
                                Designation,
                                FatherName,
                                PermanentAddress,
                                ContactNo,
                                MobileNo,
                                EMailID
                             FROM Staff
                             WHERE CollegeName=@CollegeName
                             ORDER BY IDNo";

            using SqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            await con.OpenAsync();

            DataTable dt = new DataTable();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            dt.Load(reader);

            using XLWorkbook workbook = new XLWorkbook();

            workbook.Worksheets.Add(dt, "Teachers");

            workbook.Worksheet("Teachers").Columns().AdjustToContents();

            using MemoryStream stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}

