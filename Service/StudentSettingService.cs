using ClosedXML.Excel;
using lib.DtoModel.StudentSettingDto;
using lib.Interface;
using Microsoft.Data.SqlClient;
using System.Data;

namespace lib.Service
{
    public class StudentSettingService: IStudentSettingService
    {
        private readonly IConfiguration _configuration;

        public StudentSettingService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private SqlConnection GetConnection()
        {
            return new SqlConnection(_configuration.GetConnectionString("DefaultConnection"));
        }

        public async Task<List<StudentSettingDto>> GetStudents(string collegeName)
        {
            List<StudentSettingDto> list = new();

            using SqlConnection con = GetConnection();

            string query = @"select CollegeName,
                                    IDNo,
                                    ClassRollNo,
                                    UniRollNo,
                                    StudentName,
                                    Course,
                                    Batch,
                                    FatherName,
                                    PermanentAddress,
                                    PhoneNo,
                                    StudentMobileNo,
                                    FatherMobileNo,
                                    EMailID
                             from Admissions
                             where CollegeName=@CollegeName
                             order by IDNo";

            using SqlCommand cmd = new(query, con);

            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            await con.OpenAsync();

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new StudentSettingDto
                {
                    CollegeName = dr["CollegeName"].ToString(),
                    IDNo = Convert.ToInt64(dr["IDNo"]),
                    ClassRollNo = dr["ClassRollNo"].ToString(),
                    UniRollNo = dr["UniRollNo"].ToString(),
                    StudentName = dr["StudentName"].ToString(),
                    Course = dr["Course"].ToString(),
                    Batch = dr["Batch"].ToString(),
                    FatherName = dr["FatherName"].ToString(),
                    PermanentAddress = dr["PermanentAddress"].ToString(),
                    PhoneNo = dr["PhoneNo"].ToString(),
                    StudentMobileNo = dr["StudentMobileNo"].ToString(),
                    FatherMobileNo = dr["FatherMobileNo"].ToString(),
                    EMailID = dr["EMailID"].ToString()
                });
            }

            return list;
        }

        public async Task<bool> AddStudent(StudentSettingDto dto)
        {
            using SqlConnection con = GetConnection();

            string query = @"insert into Admissions
                            (
                            CollegeName,
                            IDNo,
                            ClassRollNo,
                            UniRollNo,
                            StudentName,
                            Course,
                            Batch,
                            FatherName,
                            PermanentAddress,
                            PhoneNo,
                            StudentMobileNo,
                            FatherMobileNo,
                            EMailID
                            )

                            values

                            (
                            @CollegeName,
                            @IDNo,
                            @ClassRollNo,
                            @UniRollNo,
                            @StudentName,
                            @Course,
                            @Batch,
                            @FatherName,
                            @PermanentAddress,
                            @PhoneNo,
                            @StudentMobileNo,
                            @FatherMobileNo,
                            @EMailID
                            )";

            using SqlCommand cmd = new(query, con);

            cmd.Parameters.AddWithValue("@CollegeName", dto.CollegeName);
            cmd.Parameters.AddWithValue("@IDNo", dto.IDNo);
            cmd.Parameters.AddWithValue("@ClassRollNo", dto.ClassRollNo);
            cmd.Parameters.AddWithValue("@UniRollNo", dto.UniRollNo);
            cmd.Parameters.AddWithValue("@StudentName", dto.StudentName);
            cmd.Parameters.AddWithValue("@Course", dto.Course);
            cmd.Parameters.AddWithValue("@Batch", dto.Batch);
            cmd.Parameters.AddWithValue("@FatherName", dto.FatherName);
            cmd.Parameters.AddWithValue("@PermanentAddress", dto.PermanentAddress);
            cmd.Parameters.AddWithValue("@PhoneNo", dto.PhoneNo);
            cmd.Parameters.AddWithValue("@StudentMobileNo", dto.StudentMobileNo);
            cmd.Parameters.AddWithValue("@FatherMobileNo", dto.FatherMobileNo);
            cmd.Parameters.AddWithValue("@EMailID", dto.EMailID);

            await con.OpenAsync();

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateStudent(int oldId, StudentSettingDto dto)
        {
            using SqlConnection con = GetConnection();

            string query = @"update Admissions set

                            IDNo=@IDNo,
                            ClassRollNo=@ClassRollNo,
                            UniRollNo=@UniRollNo,
                            StudentName=@StudentName,
                            Course=@Course,
                            Batch=@Batch,
                            FatherName=@FatherName,
                            PermanentAddress=@PermanentAddress,
                            PhoneNo=@PhoneNo,
                            StudentMobileNo=@StudentMobileNo,
                            FatherMobileNo=@FatherMobileNo,
                            EMailID=@EMailID

                            where IDNo=@OldID";

            using SqlCommand cmd = new(query, con);

            cmd.Parameters.AddWithValue("@OldID", oldId);

            cmd.Parameters.AddWithValue("@IDNo", dto.IDNo);
            cmd.Parameters.AddWithValue("@ClassRollNo", dto.ClassRollNo);
            cmd.Parameters.AddWithValue("@UniRollNo", dto.UniRollNo);
            cmd.Parameters.AddWithValue("@StudentName", dto.StudentName);
            cmd.Parameters.AddWithValue("@Course", dto.Course);
            cmd.Parameters.AddWithValue("@Batch", dto.Batch);
            cmd.Parameters.AddWithValue("@FatherName", dto.FatherName);
            cmd.Parameters.AddWithValue("@PermanentAddress", dto.PermanentAddress);
            cmd.Parameters.AddWithValue("@PhoneNo", dto.PhoneNo);
            cmd.Parameters.AddWithValue("@StudentMobileNo", dto.StudentMobileNo);
            cmd.Parameters.AddWithValue("@FatherMobileNo", dto.FatherMobileNo);
            cmd.Parameters.AddWithValue("@EMailID", dto.EMailID);

            await con.OpenAsync();

            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<byte[]> ExportExcelAsync(string collegeName)
        {
            using SqlConnection con = GetConnection();

            string query = @"SELECT
                        CollegeName,
                        IDNo,
                        ClassRollNo,
                        UniRollNo,
                        StudentName,
                        Course,
                        Batch,
                        FatherName,
                        PermanentAddress,
                        PhoneNo,
                        StudentMobileNo,
                        FatherMobileNo,
                        EMailID
                    FROM Admissions
                    WHERE CollegeName=@CollegeName
                    ORDER BY IDNo";

            using SqlCommand cmd = new(query, con);
            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            await con.OpenAsync();

            DataTable dt = new();

            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            dt.Load(reader);

            using XLWorkbook workbook = new();

            workbook.Worksheets.Add(dt, "Students");

            var ws = workbook.Worksheet("Students");

            ws.Columns().AdjustToContents();

            using MemoryStream ms = new();

            workbook.SaveAs(ms);

            return ms.ToArray();
        }
    }
}

