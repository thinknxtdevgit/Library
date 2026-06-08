using ClosedXML.Excel;
using lib.DtoModel.SearchDetailedAccessionDto;
using lib.Interface;
using Microsoft.Data.SqlClient;

namespace lib.Service
{
    public class SearchDetailedAccessionService: ISearchDetailedAccessionService
    {
        private readonly string _connectionString;
        public SearchDetailedAccessionService( IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task<SearchDetailedAccessionResponseDto> SearchAccessionNoAsync(SearchDetailedAccessionRequestDto request)
        {
            var response = new SearchDetailedAccessionResponseDto();

            try
            {
                using SqlConnection con =
                    new SqlConnection(_connectionString);

                string query = @"
                    SELECT
                        CONVERT(VARCHAR(20),TransactionDate,111)
                            AS TransactionDate,

                        CONVERT(VARCHAR(8),TransactionTime,108)
                            AS TransactionTime,

                        TransactionName,
                        Title,
                        IDNo,
                        PersonName,
                        PersonType,
                        RenewalDate,
                        UserID

                    FROM Transactions

                    WHERE AccessionNo = @AccessionNo
                    AND CollegeName = @CollegeName

                    ORDER BY TransactionDate DESC";

                using SqlCommand cmd =
                    new SqlCommand(query, con);

                cmd.Parameters.AddWithValue(
                    "@AccessionNo",
                    request.AccessionNo);

                cmd.Parameters.AddWithValue(
                    "@CollegeName",
                    request.CollegeName);

                await con.OpenAsync();

                using SqlDataReader reader =
                    await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    response.Transactions.Add(
                        new TransactionHistoryDto
                        {
                            TransactionDate =
                                reader["TransactionDate"]?.ToString(),

                            TransactionTime =
                                reader["TransactionTime"]?.ToString(),

                            TransactionName =
                                reader["TransactionName"]?.ToString(),

                            Title =
                                reader["Title"]?.ToString(),

                            IDNo =
                                reader["IDNo"]?.ToString(),

                            PersonName =
                                reader["PersonName"]?.ToString(),

                            PersonType =
                                reader["PersonType"]?.ToString(),

                            RenewalDate =
                                reader["RenewalDate"] == DBNull.Value
                                ? null
                                : Convert.ToDateTime(
                                    reader["RenewalDate"]),

                            UserID =
                                reader["UserID"]?.ToString()
                        });
                }

                if (response.Transactions.Any())
                {
                    response.Success = true;
                    response.Message = "";
                }
                else
                {
                    response.Success = false;
                    response.Message =
                        "No any transactions found against this Accession No.";
                }
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.Message = ex.Message;
               
            }

            return response;
           
        }
        public async Task<byte[]> ExportAccessionHistoryAsync(SearchDetailedAccessionRequestDto request)
        {
            var result = await SearchAccessionNoAsync(request);

            if (!result.Transactions.Any())
                return null;

            using var workbook = new XLWorkbook();

            var worksheet =
                workbook.Worksheets.Add("Accession History");

            // Header Row
            worksheet.Cell(1, 1).Value = "Transaction Date";
            worksheet.Cell(1, 2).Value = "Transaction Time";
            worksheet.Cell(1, 3).Value = "Transaction Name";
            worksheet.Cell(1, 4).Value = "Title";
            worksheet.Cell(1, 5).Value = "ID No";
            worksheet.Cell(1, 6).Value = "Person Name";
            worksheet.Cell(1, 7).Value = "Person Type";
            worksheet.Cell(1, 8).Value = "Renewal Date";
            worksheet.Cell(1, 9).Value = "User ID";

            worksheet.Range("A1:I1").Style.Font.Bold = true;

            int row = 2;

            foreach (var item in result.Transactions)
            {
                worksheet.Cell(row, 1).Value = item.TransactionDate;
                worksheet.Cell(row, 2).Value = item.TransactionTime;
                worksheet.Cell(row, 3).Value = item.TransactionName;
                worksheet.Cell(row, 4).Value = item.Title;
                worksheet.Cell(row, 5).Value = item.IDNo;
                worksheet.Cell(row, 6).Value = item.PersonName;
                worksheet.Cell(row, 7).Value = item.PersonType;
                worksheet.Cell(row, 8).Value = item.RenewalDate;
                worksheet.Cell(row, 9).Value = item.UserID;

                row++;
            }

            worksheet.Columns().AdjustToContents();

            using var stream = new MemoryStream();

            workbook.SaveAs(stream);

            return stream.ToArray();
        }
    }
}

