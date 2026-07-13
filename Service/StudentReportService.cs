using lib.DtoModel.StudentReportDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class StudentReportService : IStudentReportService
    {
        private readonly string _connectionString;
        public StudentReportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<BatchDto>> GetBatchAsync(string collegeName, string course)
        {
            List<BatchDto> list = new();
            using SqlConnection con = new(_connectionString);
            await con.OpenAsync();
            SqlCommand cmd = new(@"Select Distinct Batch
                    from MasterCourse
                    where CollegeName=@College
                    and Course=@Course
                    order by Batch", con);
            cmd.Parameters.AddWithValue("@College", collegeName);
            cmd.Parameters.AddWithValue("@Course", course);
            SqlDataReader dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                list.Add(new BatchDto
                {
                    Batch = dr["Batch"].ToString()
                });
            }
            return list;
        }

        public async Task<List<CollegeDto>> GetCollegesAsync()
        {
            List<CollegeDto> list = new();
            using SqlConnection con = new(_connectionString);
            await con.OpenAsync();
            SqlCommand cmd = new(@"select distinct CollegeName
                                 from MasterCourse
                                 order by CollegeName", con);
            SqlDataReader dr = await cmd.ExecuteReaderAsync();
            while (await dr.ReadAsync())
            {
                list.Add(new CollegeDto
                {
                    CollegeName = dr["CollegeName"].ToString()
                });
            }
            return list;
        }

        public async Task<List<CourseDto>> GetCoursesAsync(string collegeName)
        {
            List<CourseDto> list = new();
            using SqlConnection con = new(_connectionString);
            await con.OpenAsync();

            SqlCommand cmd = new(@"Select Distinct Course
                    from MasterCourse
                    where CollegeName=@College
                    order by Course", con);
            cmd.Parameters.AddWithValue("@College", collegeName);
            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new CourseDto
                {
                    Course = dr["Course"].ToString()
                });
            }
            return list;
        }

        public async Task<List<StudentReportDto>> SearchAsync(StudentReportRequestDto request)
        {
            List<StudentReportDto> list = new();

            using SqlConnection con = new(_connectionString);

            await con.OpenAsync();

            string sql = @"
            select
            CollegeName,
            IDNo,
            StudentName,
            Course,
            Batch,
            FatherName,
            PermanentAddress,
            PhoneNo,
            StudentMobileNo,
            FatherMobileNo,
            MotherMobileNo
            from Admissions
            where CollegeName=@College";

            if (request.Course != "Select")
            {
                sql += " and Course=@Course";
            }

            if (request.Batch != "Select")
            {
                sql += " and Batch=@Batch";
            }

            sql += " order by IDNo";

            SqlCommand cmd = new(sql, con);

            cmd.Parameters.AddWithValue("@College", request.CollegeName);

            if (request.Course != "Select")
                cmd.Parameters.AddWithValue("@Course", request.Course);

            if (request.Batch != "Select")
                cmd.Parameters.AddWithValue("@Batch", request.Batch);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                list.Add(new StudentReportDto
                {
                    CollegeName = dr["CollegeName"].ToString(),

                    IDNo = dr["IDNo"].ToString(),

                    StudentName = dr["StudentName"].ToString(),

                    Course = dr["Course"].ToString(),

                    Batch = dr["Batch"].ToString(),

                    FatherName = dr["FatherName"].ToString(),

                    PermanentAddress = dr["PermanentAddress"].ToString(),

                    PhoneNo = dr["PhoneNo"].ToString(),

                    StudentMobileNo = dr["StudentMobileNo"].ToString(),

                    FatherMobileNo = dr["FatherMobileNo"].ToString(),

                    MotherMobileNo = dr["MotherMobileNo"].ToString()
                });
            }

            return list;
        }
    }
}


