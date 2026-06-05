using lib.DtoModel.BookHistoryDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class BookHistoryService: IBookHistoryService
    {
        private readonly string _connectionString;

        public BookHistoryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<List<BookHistoryDto>> GetBookHistoryAsync(string collegeName, string accessionNo)
        {
            var result = new List<BookHistoryDto>();

            string query = @"
            SELECT 
                CollegeName,
                CONVERT(VARCHAR(10), TransactionDate, 101) AS TransactionDate,
                RIGHT(CONVERT(VARCHAR, TransactionTime, 100), 7) AS TransactionTime,
                TransactionName,
                Type,
                AccessionNo,
                Title,
                IDNo,
                PersonName,
                PersonType
            FROM Transactions
            WHERE CollegeName = @CollegeName
              AND AccessionNo = @AccessionNo";

            using (SqlConnection con = new SqlConnection(_connectionString))
            using (SqlCommand cmd = new SqlCommand(query, con))
            {
                cmd.Parameters.AddWithValue("@CollegeName", collegeName);
                cmd.Parameters.AddWithValue("@AccessionNo", accessionNo);

                await con.OpenAsync();

                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new BookHistoryDto
                        {
                            CollegeName = reader["CollegeName"].ToString(),
                            TransactionDate = reader["TransactionDate"].ToString(),
                            TransactionTime = reader["TransactionTime"].ToString(),
                            TransactionName = reader["TransactionName"].ToString(),
                            Type = reader["Type"].ToString(),
                            AccessionNo = reader["AccessionNo"].ToString(),
                            Title = reader["Title"].ToString(),
                            IDNo = reader["IDNo"].ToString(),
                            PersonName = reader["PersonName"].ToString(),
                            PersonType = reader["PersonType"].ToString()
                        });
                    }
                }
            }

            return result;
        }
    }
}
