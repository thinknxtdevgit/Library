using lib.DtoModel.MasterIssueLimitDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class MasterIssueLimitService: IMasterIssueLimitService
    {
        private readonly string _connectionString;
        public MasterIssueLimitService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<MasterIssueLimitResponseDto> AddIssueLimitAsync(MasterIssueLimitDto dto)
        {
            MasterIssueLimitResponseDto response = new();
            using SqlConnection con = new(_connectionString);
            string sql = @"INSERT INTO MasterIssueLimit
                          (CollegeName,PersonType,IssueLimit)
                           VALUES
                          (@CollegeName,@PersonType,@IssueLimit)";
            using SqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("@CollegeName", dto.CollegeName);
            cmd.Parameters.AddWithValue("@PersonType", dto.PersonType);
            cmd.Parameters.AddWithValue("@IssueLimit", dto.IssueLimit);

            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();
            response.Success = true;
            response.Message = "Addes Successfully";
            response.Data = dto;
            return response;
        }

        public async Task<MasterIssueLimitResponseDto> GetIssueLimitAsync(string collegeName, string personType)
        {
            MasterIssueLimitResponseDto response = new();
            using SqlConnection con = new(_connectionString);
            string sql = @"SELECT IssueLimit
                        FROM MasterIssueLimit
                        WHERE CollegeName=@CollegeName AND PersonType=@PersonType";
            using SqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("@CollegeName",collegeName);
            cmd.Parameters.AddWithValue("@PersonType",personType);
            await con.OpenAsync();
            using SqlDataReader dr = await cmd.ExecuteReaderAsync();
            if(await dr.ReadAsync())
            {
                response.Success = true;
                response.Data = new MasterIssueLimitDto
                {
                    CollegeName =collegeName,
                    PersonType = personType,
                    IssueLimit = Convert.ToInt32(dr["IssueLimit"])

                };
               
            }
            else
            {
                response.Success = false;
                response.Message = "Record Not Found";
            }
            return response;
        }

        public async Task<MasterIssueLimitResponseDto> UpdateIssueLimitAsync(MasterIssueLimitDto dto)
        {
            MasterIssueLimitResponseDto response = new();
            using SqlConnection con = new(_connectionString);
            string sql = @"UPDATE MasterIssueLimit
                        SET IssueLimit=@IssueLimit
                        WHERE CollegeName=@CollegeName
                        AND PersonType=@PersonType";
            using SqlCommand cmd = new(sql, con);
            cmd.Parameters.AddWithValue("@CollegeName", dto.CollegeName);
            cmd.Parameters.AddWithValue("@PersonType", dto.PersonType);
            cmd.Parameters.AddWithValue("@IssueLimit", dto.IssueLimit);
            await con.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            response.Success = true;
            response.Message = "Update Successfully";
            response.Data = dto;

            return response;

        }
    }
}
