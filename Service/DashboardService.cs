using lib.DtoModel.DashboardDto;
using lib.Interface;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace lib.Service
{
    public class DashboardService : IDashboardService
    {
        private readonly string _connectionString;

        public DashboardService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
        }

        public async Task<DashboardStatsDto> GetDashboardStatsAsync(List<string> authorizedColleges)
        {
            var stats = new DashboardStatsDto();

            if (authorizedColleges == null || !authorizedColleges.Any())
            {
                return stats;
            }

            // Build SQL IN clause parameters dynamically
            var collegeParams = new List<string>();
            var cmdParameters = new List<SqlParameter>();
            for (int i = 0; i < authorizedColleges.Count; i++)
            {
                string paramName = $"@Col{i}";
                collegeParams.Add(paramName);
                cmdParameters.Add(new SqlParameter(paramName, authorizedColleges[i]));
            }
            string inClause = string.Join(",", collegeParams);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // 1. Total Books Count
                string totalBooksQuery = $"SELECT COUNT(*) FROM StockRegister WHERE CollegeName IN ({inClause})";
                using (SqlCommand cmd = new SqlCommand(totalBooksQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    stats.TotalBooks = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                // 2. Issued Books Count
                string issuedBooksQuery = $"SELECT COUNT(*) FROM IssueRegister WHERE CollegeName IN ({inClause})";
                using (SqlCommand cmd = new SqlCommand(issuedBooksQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    stats.IssuedBooks = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                // 3. Available Books
                stats.AvailableBooks = Math.Max(0, stats.TotalBooks - stats.IssuedBooks);

                // 4. Overdue Books Count
                string overdueBooksQuery = $"SELECT COUNT(*) FROM IssueRegister WHERE CollegeName IN ({inClause}) AND LastReturnDate < GETDATE()";
                using (SqlCommand cmd = new SqlCommand(overdueBooksQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    stats.OverdueBooks = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                // 5. Total Students Count
                string studentsQuery = $"SELECT COUNT(*) FROM Admissions WHERE CollegeName IN ({inClause})";
                using (SqlCommand cmd = new SqlCommand(studentsQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    stats.TotalStudents = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                // 6. Total Staff Count
                string staffQuery = $"SELECT COUNT(*) FROM Staff WHERE CollegeName IN ({inClause})";
                using (SqlCommand cmd = new SqlCommand(staffQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    stats.TotalStaff = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                // 7. Total Categories Count
                string categoriesQuery = $"SELECT COUNT(DISTINCT Category) FROM StockRegister WHERE CollegeName IN ({inClause})";
                using (SqlCommand cmd = new SqlCommand(categoriesQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    stats.TotalCategories = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                // 8. Total Publishers Count
                string publishersQuery = $"SELECT COUNT(DISTINCT Publisher) FROM StockRegister WHERE CollegeName IN ({inClause})";
                using (SqlCommand cmd = new SqlCommand(publishersQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    stats.TotalPublishers = (int)(await cmd.ExecuteScalarAsync() ?? 0);
                }

                // 9. Borrowing Trends (Monthly Wise - Last 12 Months)
                var monthlyTrendsMap = new Dictionary<(int Year, int Month), int>();

                try
                {
                    string trendsQuery = $@"
                        SELECT 
                            YEAR(IssueDate) AS IssueYear, 
                            MONTH(IssueDate) AS IssueMonth, 
                            COUNT(*) AS IssueCount 
                        FROM IssueRegister 
                        WHERE CollegeName IN ({inClause}) 
                          AND IssueDate IS NOT NULL 
                          AND IssueDate >= DATEADD(month, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)) 
                        GROUP BY YEAR(IssueDate), MONTH(IssueDate) 
                        ORDER BY IssueYear ASC, IssueMonth ASC";

                    using (SqlCommand cmd = new SqlCommand(trendsQuery, con))
                    {
                        cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                int yr = Convert.ToInt32(reader["IssueYear"]);
                                int mn = Convert.ToInt32(reader["IssueMonth"]);
                                int cnt = Convert.ToInt32(reader["IssueCount"]);
                                monthlyTrendsMap[(yr, mn)] = cnt;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Monthly trends query error: " + ex.Message);
                }

                // Fallback to Transactions table if IssueRegister has no monthly entries
                if (monthlyTrendsMap.Count == 0)
                {
                    try
                    {
                        string txTrendsQuery = $@"
                            SELECT 
                                YEAR(TransactionDate) AS IssueYear, 
                                MONTH(TransactionDate) AS IssueMonth, 
                                COUNT(*) AS IssueCount 
                            FROM Transactions 
                            WHERE CollegeName IN ({inClause}) 
                              AND TransactionDate IS NOT NULL 
                              AND TransactionName LIKE '%Issue%' 
                              AND TransactionDate >= DATEADD(month, -11, DATEFROMPARTS(YEAR(GETDATE()), MONTH(GETDATE()), 1)) 
                            GROUP BY YEAR(TransactionDate), MONTH(TransactionDate) 
                            ORDER BY IssueYear ASC, IssueMonth ASC";

                        using (SqlCommand cmd = new SqlCommand(txTrendsQuery, con))
                        {
                            cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    int yr = Convert.ToInt32(reader["IssueYear"]);
                                    int mn = Convert.ToInt32(reader["IssueMonth"]);
                                    int cnt = Convert.ToInt32(reader["IssueCount"]);
                                    monthlyTrendsMap[(yr, mn)] = cnt;
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Transactions monthly trends query error: " + ex.Message);
                    }
                }

                // Generate continuous 12 months trend points ending at current month
                DateTime currentMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                for (int i = 11; i >= 0; i--)
                {
                    DateTime mDate = currentMonthStart.AddMonths(-i);
                    int count = monthlyTrendsMap.ContainsKey((mDate.Year, mDate.Month)) 
                        ? monthlyTrendsMap[(mDate.Year, mDate.Month)] 
                        : 0;

                    stats.BorrowingTrends.Add(new TrendPointDto
                    {
                        Date = mDate.ToString("MMM yyyy"),
                        Count = count
                    });
                }

                // 10. Recent Activities (Real Dynamic Data from Transactions, IssueRegister & StockRegister)
                var rawActivities = new List<(DateTime Time, ActivityDto Act)>();

                // A. Real Transactions Table Query
                try
                {
                    string transactionsQuery = $@"
                        SELECT TOP 5 
                            TransactionDate,
                            TransactionTime,
                            TransactionName,
                            Type,
                            AccessionNo,
                            Title,
                            IDNo,
                            PersonName,
                            PersonType
                        FROM Transactions
                        WHERE CollegeName IN ({inClause})
                        ORDER BY TransactionDate DESC";

                    using (SqlCommand cmd = new SqlCommand(transactionsQuery, con))
                    {
                        cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                string tName = reader["TransactionName"]?.ToString() ?? "Transaction";
                                string title = reader["Title"]?.ToString() ?? "Book";
                                string accNo = reader["AccessionNo"]?.ToString() ?? "";
                                string person = reader["PersonName"]?.ToString() ?? "";
                                string pType = reader["PersonType"]?.ToString() ?? "";
                                string dateStr = reader["TransactionDate"]?.ToString() ?? "";
                                string timeStr = reader["TransactionTime"]?.ToString() ?? "";

                                DateTime actTime = DateTime.Now;
                                if (DateTime.TryParse($"{dateStr} {timeStr}", out DateTime parsedDt))
                                {
                                    actTime = parsedDt;
                                }
                                else if (reader["TransactionDate"] != DBNull.Value && reader["TransactionDate"] is DateTime dtVal)
                                {
                                    actTime = dtVal;
                                }

                                string status = "info";
                                string lowerName = tName.ToLower();
                                if (lowerName.Contains("return")) status = "success";
                                else if (lowerName.Contains("renew")) status = "warning";
                                else if (lowerName.Contains("issue")) status = "info";
                                else status = "primary";

                                string details = string.IsNullOrEmpty(person)
                                    ? $"\"{title}\" (Accession No. {accNo})"
                                    : $"\"{title}\" (Acc No. {accNo}) - {person} ({pType})";

                                rawActivities.Add((actTime, new ActivityDto
                                {
                                    Title = tName,
                                    Details = details,
                                    TimeAgo = FormatTimeAgo(actTime),
                                    Status = status
                                }));
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Transactions query error: " + ex.Message);
                }

                // B. If Transactions table has fewer than 5 items, supplement with recent IssueRegister entries
                if (rawActivities.Count < 5)
                {
                    try
                    {
                        string recentIssuesQuery = $@"
                            SELECT TOP 5 IssueDate, WhomIssued, Title, AccessionNo 
                            FROM IssueRegister 
                            WHERE CollegeName IN ({inClause}) 
                            ORDER BY IssueDate DESC";

                        using (SqlCommand cmd = new SqlCommand(recentIssuesQuery, con))
                        {
                            cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    DateTime issueDate = reader["IssueDate"] == DBNull.Value ? DateTime.Now : Convert.ToDateTime(reader["IssueDate"]);
                                    string whom = reader["WhomIssued"]?.ToString() ?? "Borrower";
                                    string title = reader["Title"]?.ToString() ?? "Unknown Title";
                                    string accNo = reader["AccessionNo"]?.ToString() ?? "";

                                    rawActivities.Add((issueDate, new ActivityDto
                                    {
                                        Title = "Book Issued",
                                        Details = $"{whom} checked out \"{title}\" (Acc No. {accNo}).",
                                        TimeAgo = FormatTimeAgo(issueDate),
                                        Status = "info"
                                    }));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Recent issues query error: " + ex.Message);
                    }
                }

                // C. If still fewer than 5 items, supplement with recent StockRegister entries
                if (rawActivities.Count < 5)
                {
                    try
                    {
                        string recentBooksQuery = $@"
                            SELECT TOP 5 Title, AccessionNo, DateEntry 
                            FROM StockRegister 
                            WHERE CollegeName IN ({inClause}) 
                            ORDER BY DateEntry DESC, TRY_CAST(AccessionNo AS INT) DESC";

                        using (SqlCommand cmd = new SqlCommand(recentBooksQuery, con))
                        {
                            cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                            using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    string title = reader["Title"]?.ToString() ?? "Unknown Title";
                                    string accNo = reader["AccessionNo"]?.ToString() ?? "";
                                    DateTime entryDate = (reader["DateEntry"] != DBNull.Value && reader["DateEntry"] is DateTime dt)
                                        ? dt
                                        : DateTime.Now;

                                    rawActivities.Add((entryDate, new ActivityDto
                                    {
                                        Title = "Book Stock Registered",
                                        Details = $"\"{title}\" (Accession No. {accNo}) registered in catalog.",
                                        TimeAgo = FormatTimeAgo(entryDate),
                                        Status = "success"
                                    }));
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Recent books query error: " + ex.Message);
                    }
                }

                // Sort merged real activities by date descending, deduplicate, and take top 5
                stats.RecentActivities = rawActivities
                    .OrderByDescending(x => x.Time)
                    .Select(x => x.Act)
                    .GroupBy(x => x.Details)
                    .Select(g => g.First())
                    .Take(5)
                    .ToList();
            }

            return stats;
        }

        private string FormatTimeAgo(DateTime dt)
        {
            var span = DateTime.Now - dt;
            if (span.TotalDays > 1) return $"{Math.Floor(span.TotalDays)} days ago";
            if (span.TotalHours > 1) return $"{Math.Floor(span.TotalHours)} hours ago";
            if (span.TotalMinutes > 1) return $"{Math.Floor(span.TotalMinutes)} mins ago";
            return "Just now";
        }

        public async Task<List<QuickSearchResultDto>> QuickSearchAsync(List<string> authorizedColleges, string query)
        {
            var results = new List<QuickSearchResultDto>();

            if (string.IsNullOrWhiteSpace(query) || authorizedColleges == null || !authorizedColleges.Any())
            {
                return results;
            }

            query = query.Trim();

            var collegeParams = new List<string>();
            var cmdParameters = new List<SqlParameter>();
            for (int i = 0; i < authorizedColleges.Count; i++)
            {
                string paramName = $"@Col{i}";
                collegeParams.Add(paramName);
                cmdParameters.Add(new SqlParameter(paramName, authorizedColleges[i]));
            }
            string inClause = string.Join(",", collegeParams);

            using (SqlConnection con = new SqlConnection(_connectionString))
            {
                await con.OpenAsync();

                // 1. Search Books
                string booksQuery = $@"
                    SELECT TOP 5 Title, AccessionNo, Author 
                    FROM StockRegister 
                    WHERE CollegeName IN ({inClause}) 
                    AND (Title LIKE '%' + @Query + '%' OR AccessionNo = @Query OR Author LIKE '%' + @Query + '%')";

                using (SqlCommand cmd = new SqlCommand(booksQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    cmd.Parameters.AddWithValue("@Query", query);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new QuickSearchResultDto
                            {
                                Type = "Book",
                                Id = reader["AccessionNo"]?.ToString() ?? "",
                                Title = reader["Title"]?.ToString() ?? "",
                                Subtitle = $"Author: {reader["Author"]?.ToString() ?? "Unknown"}"
                            });
                        }
                    }
                }

                // 2. Search Students
                string studentsQuery = $@"
                    SELECT TOP 5 StudentName, IDNo, ClassRollNo 
                    FROM Admissions 
                    WHERE CollegeName IN ({inClause}) 
                    AND (StudentName LIKE '%' + @Query + '%' OR IDNo = @Query OR ClassRollNo = @Query)";

                using (SqlCommand cmd = new SqlCommand(studentsQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    cmd.Parameters.AddWithValue("@Query", query);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(new QuickSearchResultDto
                            {
                                Type = "Student",
                                Id = reader["IDNo"]?.ToString() ?? "",
                                Title = reader["StudentName"]?.ToString() ?? "",
                                Subtitle = $"Roll No: {reader["ClassRollNo"]?.ToString() ?? "N/A"}"
                            });
                        }
                    }
                }
            }

            return results;
        }
    }
}
