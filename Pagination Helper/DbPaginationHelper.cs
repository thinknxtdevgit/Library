
using Microsoft.Data.SqlClient;

namespace lib.Pagination_Helper
{
    public class DbPaginationHelper
    {
        private readonly string _connectionString;

        public DbPaginationHelper(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<PagedResult<T>> GetPagedResultAsync<T>(
            string dataQuery,
            string countQuery,
            SqlParameter[] parameters,
            int pageNumber,
            int pageSize,
            Func<SqlDataReader, T> map)
        {
            var list = new List<T>();
            int totalRecords = 0;

            using SqlConnection con = new SqlConnection(_connectionString);

            // 🔹 COUNT
            using (SqlCommand countCmd = new SqlCommand(countQuery, con))
            {
                if (parameters != null)
                    countCmd.Parameters.AddRange(parameters);

                await con.OpenAsync();
                totalRecords = (int)await countCmd.ExecuteScalarAsync();
            }

            int offset = PaginationHelper.GetOffset(pageNumber, pageSize);

            // 🔹 DATA
            using SqlCommand cmd = new SqlCommand(dataQuery, con);

            if (parameters != null)
                cmd.Parameters.AddRange(parameters);

            cmd.Parameters.AddWithValue("@Offset", offset);
            cmd.Parameters.AddWithValue("@PageSize", pageSize);

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add(map(reader));
            }

            return new PagedResult<T>
            {
                Data = list,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }
    }
}
