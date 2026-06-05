using lib.DtoModel.ReferenceBookDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class ReferenceBookService : IReferenceBookService
    {
        private readonly string _connectionString;
        public ReferenceBookService(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("DefaultConnection");
        }
        public async Task<ReferenceBookResponseDto>
     GetReferenceBooksAsync(string collegeName)
        {
            var response =
                new ReferenceBookResponseDto();

            using SqlConnection con =
                new SqlConnection(_connectionString);

            await con.OpenAsync();

            string query = @"

SELECT
DateEntry,
AccessionNo,
Title,
Author,
Edition,
Publisher,
Year,
Pages,
Volume,
Source,
Price,
NetPrice,
Discount,
Type,
Category,
BillNo,
ClassNo,
BillDate,
BookNo,
Remarks,
Location,
FirstName,
SirName,
FirstAuthorForeName,
FirstAuthorSirName,
SecondAuthorForeName,
SecondAuthorSirName,
ThirdAuthorForeName,
ThirdAuthorSirName,
MoreThanThreeAuthors,
SubTitle,
ISBN,
Place,
Series,
BookSize,
Subject1,
Subject2
FROM StockRegister
WHERE CollegeName=@CollegeName
AND Type='Reference'
ORDER BY AccessionNo";

            using SqlCommand cmd =
                new SqlCommand(query, con);

            cmd.Parameters.AddWithValue(
                "@CollegeName",
                collegeName);

            using SqlDataReader reader =
                await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                response.Books.Add(
                    new ReferenceBookDto
                    {
                        DateEntry =
                            reader["DateEntry"] == DBNull.Value
                            ? null
                            : Convert.ToDateTime(reader["DateEntry"]),

                        AccessionNo =
                            reader["AccessionNo"]?.ToString(),

                        Title =
                            reader["Title"]?.ToString(),

                        Author =
                            reader["Author"]?.ToString(),

                        Edition =
                            reader["Edition"]?.ToString(),

                        Publisher =
                            reader["Publisher"]?.ToString(),

                        Year =
                            reader["Year"]?.ToString(),

                        Pages =
                            reader["Pages"]?.ToString(),

                        Volume =
                            reader["Volume"]?.ToString(),

                        Source =
                            reader["Source"]?.ToString(),

                        Price =
                            reader["Price"] == DBNull.Value
                            ? null
                            : Convert.ToDecimal(reader["Price"]),

                        NetPrice =
                            reader["NetPrice"] == DBNull.Value
                            ? null
                            : Convert.ToDecimal(reader["NetPrice"]),

                        Discount =
                            reader["Discount"] == DBNull.Value
                            ? null
                            : Convert.ToDecimal(reader["Discount"]),

                        Type =
                            reader["Type"]?.ToString(),

                        Category =
                            reader["Category"]?.ToString(),

                        BillNo =
                            reader["BillNo"]?.ToString(),

                        ClassNo =
                            reader["ClassNo"]?.ToString(),

                        BillDate =
                            reader["BillDate"]?.ToString(),

                        BookNo =
                            reader["BookNo"]?.ToString(),

                        Remarks =
                            reader["Remarks"]?.ToString(),

                        Location =
                            reader["Location"]?.ToString(),

                        FirstName =
                            reader["FirstName"]?.ToString(),

                        SirName =
                            reader["SirName"]?.ToString(),

                        FirstAuthorForeName =
                            reader["FirstAuthorForeName"]?.ToString(),

                        FirstAuthorSirName =
                            reader["FirstAuthorSirName"]?.ToString(),

                        SecondAuthorForeName =
                            reader["SecondAuthorForeName"]?.ToString(),

                        SecondAuthorSirName =
                            reader["SecondAuthorSirName"]?.ToString(),

                        ThirdAuthorForeName =
                            reader["ThirdAuthorForeName"]?.ToString(),

                        ThirdAuthorSirName =
                            reader["ThirdAuthorSirName"]?.ToString(),

                        MoreThanThreeAuthors =
                            reader["MoreThanThreeAuthors"]?.ToString(),

                        SubTitle =
                            reader["SubTitle"]?.ToString(),

                        ISBN =
                            reader["ISBN"]?.ToString(),

                        Place =
                            reader["Place"]?.ToString(),

                        Series =
                            reader["Series"]?.ToString(),

                        BookSize =
                            reader["BookSize"]?.ToString(),

                        Subject1 =
                            reader["Subject1"]?.ToString(),

                        Subject2 =
                            reader["Subject2"]?.ToString()
                    });
            }

            response.TotalRecords =
                response.Books.Count;

            return response;
        }
    }
}
