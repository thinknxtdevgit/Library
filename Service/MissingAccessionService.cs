using ClosedXML.Excel;
using lib.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Threading.Tasks;

namespace lib.Service
{
    public class MissingAccessionService : BaseService, IMissingAccessionService
    {
        private readonly string _connectionString;

        public MissingAccessionService(
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor) : base(httpContextAccessor)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        // ========================================================
        // Generate Temp numbers and Find Missing Accession numbers
        // ========================================================
        public async Task<List<int>> GenerateAndFindMissingAsync(string collegeName)
        {
            List<int> missingNumbers = new List<int>();

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // 1. Get Max Accession Number
                int varmaxaccessionNo = 1;
                string maxQuery = "Select Max(AccessionNo) From StockRegister Where CollegeName = @CollegeName";
                using (SqlCommand cmdMax = new SqlCommand(maxQuery, con))
                {
                    cmdMax.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);
                    var maxObj = await cmdMax.ExecuteScalarAsync();
                    if (maxObj != null && maxObj != DBNull.Value)
                    {
                        if (int.TryParse(maxObj.ToString(), out int parsedMax))
                        {
                            varmaxaccessionNo = parsedMax;
                        }
                    }
                }

                // 2. Clear and Insert into TempAcc in a single transaction
                using (SqlTransaction transaction = con.BeginTransaction())
                {
                    try
                    {
                        // Delete existing temp records for this college
                        string deleteQuery = "Delete From TempAcc Where CollegeName = @CollegeName";
                        using (SqlCommand cmdDelete = new SqlCommand(deleteQuery, con, transaction))
                        {
                            cmdDelete.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);
                            await cmdDelete.ExecuteNonQueryAsync();
                        }

                        // Bulk insert new temp accession numbers
                        string insertQuery = "Insert Into TempAcc(CollegeName,AccessionNo) values(@CollegeName,@AccessionNo)";
                        using (SqlCommand cmdInsert = new SqlCommand(insertQuery, con, transaction))
                        {
                            cmdInsert.Parameters.Add("@CollegeName", SqlDbType.NVarChar);
                            cmdInsert.Parameters.Add("@AccessionNo", SqlDbType.Int);

                            cmdInsert.Parameters["@CollegeName"].Value = collegeName ?? string.Empty;

                            for (int i = 1; i <= varmaxaccessionNo; i++)
                            {
                                cmdInsert.Parameters["@AccessionNo"].Value = i;
                                await cmdInsert.ExecuteNonQueryAsync();
                            }
                        }

                        await transaction.CommitAsync();
                    }
                    catch
                    {
                        await transaction.RollbackAsync();
                        throw;
                    }
                }

                // 3. Find Missing Accession Numbers
                string missingQuery = @"
                    Select AccessionNo 
                    from TempAcc 
                    where CollegeName = @CollegeName 
                    And AccessionNo Not In (
                        Select AccessionNo from StockRegister where CollegeName = @CollegeName
                    ) 
                    order By AccessionNo asc";

                using (SqlCommand cmdMissing = new SqlCommand(missingQuery, con))
                {
                    cmdMissing.Parameters.AddWithValue("@CollegeName", collegeName ?? (object)DBNull.Value);
                    cmdMissing.CommandTimeout = 0; // matching cmd.CommandTimeout = 0

                    using (SqlDataReader reader = await cmdMissing.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (reader.GetValue(0) != DBNull.Value)
                            {
                                missingNumbers.Add(Convert.ToInt32(reader.GetValue(0)));
                            }
                        }
                    }
                }
            }

            return missingNumbers;
        }

        // ==========================================
        // Export to Excel using ClosedXML
        // ==========================================
        public async Task<byte[]> ExportExcelAsync(string collegeName)
        {
            var missingList = await GenerateAndFindMissingAsync(collegeName);

            using (XLWorkbook workbook = new XLWorkbook())
            {
                var sheet = workbook.Worksheets.Add("Missing Accession Numbers");

                // Headers
                sheet.Cell(1, 1).Value = "College Name";
                sheet.Cell(1, 2).Value = "Missing Accession No.";

                // Headers styling
                var headerRange = sheet.Range(1, 1, 1, 2);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#D9534F"); // Red Accent
                headerRange.Style.Font.FontColor = XLColor.White;

                int row = 2;
                foreach (var accessionNo in missingList)
                {
                    sheet.Cell(row, 1).SetValue(collegeName);
                    sheet.Cell(row, 2).SetValue(accessionNo);
                    row++;
                }

                sheet.Columns().AdjustToContents();

                using (MemoryStream ms = new MemoryStream())
                {
                    workbook.SaveAs(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
