using lib.DtoModel.SearchPersonIdDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class SearchPersonIdService: ISearchPersonIdService
    {
        private readonly string _connectionString;
        public SearchPersonIdService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }
        // ==========================
        // Load Colleges
        // ==========================
        public async Task<List<string>> GetCollegesAsync()
        {
            List<string> colleges = new();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            string query = @"
                SELECT DISTINCT CollegeName
                FROM Transactions
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

        // ==========================
        // Search Person Transaction
        // ==========================
        public async Task<PersonTransactionResponseDto> SearchAsync(
            string collegeName,
            string personId)
        {
            PersonTransactionResponseDto response =
                new PersonTransactionResponseDto();

            try
            {
                List<PersonTransactionDto> list = new();

                using SqlConnection con =
                    new SqlConnection(_connectionString);

                string query = @"
                SELECT
                    CONVERT(VARCHAR(20),TransactionDate,103) TransactionDate,
                    CONVERT(VARCHAR,TransactionTime,108) TransactionTime,
                    TransactionName,
                    Type,
                    AccessionNo,
                    Title,
                    IDNo,
                    PersonName,
                    PersonType,
                    RenewalDate
                FROM Transactions
                WHERE IDNo = @ID
                AND CollegeName = @CollegeName
                ORDER BY TransactionDate DESC";

                using SqlCommand cmd =
                    new SqlCommand(query, con);

                cmd.Parameters.AddWithValue("@ID", personId);
                cmd.Parameters.AddWithValue("@CollegeName", collegeName);

                await con.OpenAsync();

                using SqlDataReader reader =
                    await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    list.Add(new PersonTransactionDto
                    {
                        TransactionDate = reader["TransactionDate"]?.ToString(),
                        TransactionTime = reader["TransactionTime"]?.ToString(),
                        TransactionName = reader["TransactionName"]?.ToString(),
                        Type = reader["Type"]?.ToString(),
                        AccessionNo = reader["AccessionNo"]?.ToString(),
                        Title = reader["Title"]?.ToString(),
                        IDNo = reader["IDNo"]?.ToString(),
                        PersonName = reader["PersonName"]?.ToString(),
                        PersonType = reader["PersonType"]?.ToString(),
                        RenewalDate = reader["RenewalDate"]?.ToString()
                    });
                }

                if (list.Any())
                {
                    response.Success = true;
                    response.Message = "Records Found";
                    response.Data = list;
                }
                else
                {
                    response.Success = false;
                    response.Message = "No Records are Found";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
            }

            return response;
        }
    }
}

