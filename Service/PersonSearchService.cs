using lib.DtoModel.PersonDetailDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class PersonSearchService: IPersonSearchService
    {
        private readonly string _connectionString;

        public PersonSearchService(IConfiguration config)
        {
            _connectionString =
                config.GetConnectionString("DefaultConnection");
        }
        // ========================Main Method==============================
        public async Task<PersonSearchResponseDto> SearchPersonAsync(string idNo, bool isUniversityRollNo)

        {
            if (isUniversityRollNo)
            {
                idNo = await GetPersonIdNo(idNo);
            }

            if (string.IsNullOrEmpty(idNo))
                return null;

            if (idNo.Length == 6)
            {
                return await SearchStaffAsync(idNo);
            }

            return await SearchStudentAsync(idNo);
        }
        // ========================Search Student==============================
        private async Task<PersonSearchResponseDto> SearchStudentAsync(string idNo)

        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = @"
    SELECT StudentName,
           Course,
           Snap,
           Batch,
           CollegeName,
           ClassRollNo
    FROM Admissions
    WHERE IDNo=@IDNo";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return await SearchCancelledStudentAsync(idNo);
            }

            var dto = new PersonSearchResponseDto
            {
                CollegeName = reader["CollegeName"]?.ToString(),
                Name = reader["StudentName"]?.ToString(),
                CourseOrDesignation = reader["Course"]?.ToString(),
                BatchOrDepartment = reader["Batch"]?.ToString(),
                RollNoOrIdNo = reader["ClassRollNo"]?.ToString(),
                PersonType = "Student",
                Snap = reader["Snap"] as byte[]
            };

            dto.Books =
                await GetBookDetailAsync(idNo);

            dto.CDs =
                await GetCDDetailAsync(idNo);

            dto.Fines =
                await GetFineDetailAsync(idNo);

            return dto;
        }
        // =======================Search Staff===============================
        private async Task<PersonSearchResponseDto> SearchStaffAsync(string idNo)

        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = @"
    SELECT Name,
           Designation,
           Snap,
           Department,
           CollegeName,
           IDNo
    FROM Staff
    WHERE IDNo=@IDNo";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return await SearchLeftStaffAsync(idNo);
            }

            var dto = new PersonSearchResponseDto
            {
                CollegeName = reader["CollegeName"]?.ToString(),
                Name = reader["Name"]?.ToString(),
                CourseOrDesignation = reader["Designation"]?.ToString(),
                BatchOrDepartment = reader["Department"]?.ToString(),
                RollNoOrIdNo = reader["IDNo"]?.ToString(),
                PersonType = "Staff",
                Snap = reader["Snap"] as byte[]
            };

            dto.Books =
                await GetBookDetailAsync(idNo);

            dto.CDs =
                await GetCDDetailAsync(idNo);

            dto.Fines =
                await GetFineDetailAsync(idNo);

            return dto;
        }

        // ======================GetPersonIdNo================================
        private async Task<string> GetPersonIdNo(string uniRollNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = @"
        SELECT IDNo
        FROM Admissions
        WHERE UniRollNo = @UniRollNo";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@UniRollNo", uniRollNo);

            await con.OpenAsync();

            var result = await cmd.ExecuteScalarAsync();

            return result?.ToString() ?? "";
        }



        // =======================Search Left Staff===============================
        private async Task<PersonSearchResponseDto> SearchLeftStaffAsync(string idNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = @"
        SELECT Name,
               Designation,
               Snap,
               Department,
               CollegeName,
               IDNo
        FROM EmployeeLeft
        WHERE IDNo=@IDNo";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var dto = new PersonSearchResponseDto
            {
                CollegeName = reader["CollegeName"]?.ToString(),
                Name = reader["Name"]?.ToString(),
                CourseOrDesignation = reader["Designation"]?.ToString(),
                BatchOrDepartment = reader["Department"]?.ToString(),
                RollNoOrIdNo = reader["IDNo"]?.ToString(),
                PersonType = "LeftStaff",
                Snap = reader["Snap"] as byte[]
            };

            dto.Books = await GetBookDetailAsync(idNo);
            dto.CDs = await GetCDDetailAsync(idNo);
            dto.Fines = await GetFineDetailAsync(idNo);

            return dto;
        }

        // ======================Search Cancelled Admission================================
        private async Task<PersonSearchResponseDto> SearchCancelledStudentAsync(string idNo)
        {
            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = @"
        SELECT StudentName,
               Course,
               Snap,
               Batch,
               CollegeName,
               ClassRollNo
        FROM CancelledAdmission
        WHERE IDNo=@IDNo";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
                return null;

            var dto = new PersonSearchResponseDto
            {
                CollegeName = reader["CollegeName"]?.ToString(),
                Name = reader["StudentName"]?.ToString(),
                CourseOrDesignation = reader["Course"]?.ToString(),
                BatchOrDepartment = reader["Batch"]?.ToString(),
                RollNoOrIdNo = reader["ClassRollNo"]?.ToString(),
                PersonType = "CancelledStudent",
                Snap = reader["Snap"] as byte[]
            };

            dto.Books = await GetBookDetailAsync(idNo);
            dto.CDs = await GetCDDetailAsync(idNo);
            dto.Fines = await GetFineDetailAsync(idNo);

            return dto;
        }


        // ======================Get Books================================
        private async Task<List<BookIssuedDto>>
    GetBookDetailAsync(string idNo)
        {
            var list = new List<BookIssuedDto>();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = @"
    SELECT AccessionNo,
           Title,
           Author,
           IssueDate,
           LastReturnDate,
           Condition,
           Remarks
    FROM IssueRegister
    WHERE IDNo=@IDNo";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new BookIssuedDto
                {
                    AccessionNo = reader["AccessionNo"]?.ToString(),
                    Title = reader["Title"]?.ToString(),
                    Author = reader["Author"]?.ToString(),
                    IssueDate = reader["IssueDate"] as DateTime?,
                    LastReturnDate = reader["LastReturnDate"] as DateTime?,
                    Condition = reader["Condition"]?.ToString(),
                    Remarks = reader["Remarks"]?.ToString()
                });
            }

            return list;
        }
        // ======================Get CDs================================
        private async Task<List<CDIssuedDto>> GetCDDetailAsync(string idNo)
        {
            var list = new List<CDIssuedDto>();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = @"
        SELECT SerialNo,
               Title,
               IssueType,
               IssueDate,
               MagazineID,
               Remarks
        FROM IssueRegisterCD
        WHERE IDNo=@IDNo";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new CDIssuedDto
                {
                    SerialNo = reader["SerialNo"]?.ToString(),
                    Title = reader["Title"]?.ToString(),
                    IssueType = reader["IssueType"]?.ToString(),
                    IssueDate = reader["IssueDate"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["IssueDate"]),
                    MagazineID = reader["MagazineID"]?.ToString(),
                    Remarks = reader["Remarks"]?.ToString()
                });
            }

            return list;
        }
        // ======================Get Fine Details================================
        private async Task<List<FineDetailDto>> GetFineDetailAsync(string idNo)
        {
            var list = new List<FineDetailDto>();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            string sql = @"
        SELECT AccessionNo,
               Title,
               Author,
               DateOfIssue,
               LastReturnDate,
               DateOfFine,
               Fine,
               FineStatus
        FROM FineRegister
        WHERE IDNo=@IDNo";

            using SqlCommand cmd =
                new SqlCommand(sql, con);

            cmd.Parameters.AddWithValue("@IDNo", idNo);

            await con.OpenAsync();

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(new FineDetailDto
                {
                    AccessionNo = reader["AccessionNo"]?.ToString(),
                    Title = reader["Title"]?.ToString(),
                    Author = reader["Author"]?.ToString(),

                    DateOfIssue = reader["DateOfIssue"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["DateOfIssue"]),

                    LastReturnDate = reader["LastReturnDate"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["LastReturnDate"]),

                    DateOfFine = reader["DateOfFine"] == DBNull.Value
                        ? null
                        : Convert.ToDateTime(reader["DateOfFine"]),

                    Fine = reader["Fine"] == DBNull.Value
                        ? 0
                        : Convert.ToDecimal(reader["Fine"]),

                    FineStatus = reader["FineStatus"]?.ToString()
                });
            }

            return list;
        }
    }


}
