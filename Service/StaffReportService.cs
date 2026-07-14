using lib.DtoModel.StaffReportDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class StaffReportService:IStaffReportService
    {
        private readonly string _connectionString;
        public StaffReportService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        public async Task<StaffReportResponseDto> SearchAsync(StaffReportRequestDto request)
        {
            StaffReportResponseDto response = new();

            using SqlConnection con = new(_connectionString);

            await con.OpenAsync();

            string sql = @"
            SELECT
                CollegeName,
                IDNo,
                Name,
                Designation,
                Department,
                PermanentAddress,
                ContactNo,
                MobileNo
            FROM Staff
            WHERE CollegeName=@CollegeName
            ORDER BY IDNo";

            SqlCommand cmd = new(sql, con);

            cmd.Parameters.AddWithValue("@CollegeName", request.CollegeName);

            SqlDataReader dr = await cmd.ExecuteReaderAsync();

            while (await dr.ReadAsync())
            {
                response.StaffList.Add(new StaffReportDto
                {
                    CollegeName = dr["CollegeName"].ToString(),

                    IDNo = dr["IDNo"].ToString(),

                    Name = dr["Name"].ToString(),

                    Designation = dr["Designation"].ToString(),

                    Department = dr["Department"].ToString(),

                    PermanentAddress = dr["PermanentAddress"].ToString(),

                    ContactNo = dr["ContactNo"].ToString(),

                    MobileNo = dr["MobileNo"].ToString()
                });
            }

            await dr.CloseAsync();

            response.CollegeName = request.CollegeName;

            response.TotalRecords = response.StaffList.Count;

            // Equivalent of VB functions
            //response.Address1 = await GetCollegeAddress1(con, request.CollegeName);

            //response.Address2 = await GetCollegeAddress2(con, request.CollegeName);

            return response;
        }

        private async Task<string> GetCollegeAddress1(SqlConnection con, string college)
        {
            string sql = @"SELECT Address1
                           FROM CollegeMaster
                           WHERE CollegeName=@College";

            SqlCommand cmd = new(sql, con);

            cmd.Parameters.AddWithValue("@College", college);

            object obj = await cmd.ExecuteScalarAsync();

            return obj?.ToString() ?? "";
        }

        private async Task<string> GetCollegeAddress2(SqlConnection con, string college)
        {
            string sql = @"SELECT Address2
                           FROM CollegeMaster
                           WHERE CollegeName=@College";

            SqlCommand cmd = new(sql, con);

            cmd.Parameters.AddWithValue("@College", college);

            object obj = await cmd.ExecuteScalarAsync();

            return obj?.ToString() ?? "";
        }
    }
}

