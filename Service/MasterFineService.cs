using lib.DtoModel.MasterFineDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class MasterFineService : IMasterFineService
    {
        private readonly string _connectionString;
        public MasterFineService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }
        public async Task<MasterFineResponseDto> GetFineAsync(string collegeName)
        {
            var response = new MasterFineResponseDto();

            using SqlConnection con = new(_connectionString);

            string sql = @"SELECT FinePerDay
                           FROM MasterFine
                           WHERE CollegeName=@CollegeName";

            using SqlCommand cmd = new(sql, con);

            cmd.Parameters.AddWithValue("@CollegeName", collegeName);

            await con.OpenAsync();

            using SqlDataReader dr = await cmd.ExecuteReaderAsync();

            if (await dr.ReadAsync())
            {
                response.Success = true;

                response.Data = new MasterFineDto
                {
                    CollegeName = collegeName,
                    FinePerDay = Convert.ToDecimal(dr["FinePerDay"])
                };
            }
            else
            {
                response.Success = false;
                response.Message = "Record not found";
            }

            return response;
        }

        public async Task<MasterFineResponseDto> AddFineAsync(MasterFineDto dto)
        {
            var response = new MasterFineResponseDto();

            using SqlConnection con = new(_connectionString);

            string sql = @"INSERT INTO MasterFine
                           (CollegeName,FinePerDay)
                           VALUES
                           (@CollegeName,@FinePerDay)";

            using SqlCommand cmd = new(sql, con);

            cmd.Parameters.AddWithValue("@CollegeName", dto.CollegeName);

            cmd.Parameters.AddWithValue("@FinePerDay", dto.FinePerDay);

            await con.OpenAsync();

            await cmd.ExecuteNonQueryAsync();

            response.Success = true;

            response.Message = "Added Successfully";

            return response;
        }

        public async Task<MasterFineResponseDto> UpdateFineAsync(MasterFineDto dto)
        {
            var response = new MasterFineResponseDto();

            using SqlConnection con = new(_connectionString);

            string sql = @"UPDATE MasterFine
                           SET FinePerDay=@FinePerDay
                           WHERE CollegeName=@CollegeName";

            using SqlCommand cmd = new(sql, con);

            cmd.Parameters.AddWithValue("@CollegeName", dto.CollegeName);

            cmd.Parameters.AddWithValue("@FinePerDay", dto.FinePerDay);

            await con.OpenAsync();

            await cmd.ExecuteNonQueryAsync();

            response.Success = true;

            response.Message = "Updated Successfully";
            response.Data = dto;

            return response;
        }
    }
}

