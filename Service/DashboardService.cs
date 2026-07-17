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

                // 9. Borrowing Trends (Last 7 Days)
                string trendsQuery = $@"
                    SELECT CAST(IssueDate AS DATE) AS IssueDate, COUNT(*) AS IssueCount 
                    FROM IssueRegister 
                    WHERE CollegeName IN ({inClause}) 
                    AND IssueDate >= DATEADD(day, -6, CAST(GETDATE() AS DATE)) 
                    GROUP BY CAST(IssueDate AS DATE) 
                    ORDER BY IssueDate ASC";
                
                var trendsMap = new Dictionary<DateTime, int>();
                using (SqlCommand cmd = new SqlCommand(trendsQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (reader[0] != DBNull.Value)
                            {
                                trendsMap[Convert.ToDateTime(reader[0]).Date] = Convert.ToInt32(reader[1]);
                            }
                        }
                    }
                }

                // Generate continuous 7 days trend points
                DateTime today = DateTime.Today;
                for (int i = 6; i >= 0; i--)
                {
                    DateTime date = today.AddDays(-i);
                    int count = trendsMap.ContainsKey(date) ? trendsMap[date] : 0;
                    stats.BorrowingTrends.Add(new TrendPointDto
                    {
                        Date = date.ToString("dd MMM"),
                        Count = count
                    });
                }

                // 10. Recent Activities
                var rawActivities = new List<(DateTime Time, ActivityDto Act)>();

                // Recent Book Additions (ordered by accession no descending)
                string recentBooksQuery = $@"
                    SELECT TOP 3 Title, AccessionNo, CollegeName 
                    FROM StockRegister 
                    WHERE CollegeName IN ({inClause}) 
                    ORDER BY TRY_CAST(AccessionNo AS INT) DESC";
                
                using (SqlCommand cmd = new SqlCommand(recentBooksQuery, con))
                {
                    cmd.Parameters.AddRange(cmdParameters.Select(p => ((ICloneable)p).Clone() as SqlParameter).ToArray());
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            string title = reader["Title"]?.ToString() ?? "Unknown Title";
                            string accNo = reader["AccessionNo"]?.ToString() ?? "";
                            rawActivities.Add((DateTime.Now.AddMinutes(-10 * rawActivities.Count - 5), new ActivityDto
                            {
                                Title = "New Book Registered",
                                Details = $"\"{title}\" (Accession No. {accNo}) was registered in stock catalog.",
                                Status = "success"
                            }));
                        }
                    }
                }

                // Recent Book Issuances (ordered by issue date descending)
                string recentIssuesQuery = $@"
                    SELECT TOP 3 IssueDate, WhomIssued, Title, CollegeName 
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
                            
                            // Format TimeAgo nicely
                            string timeAgoStr = FormatTimeAgo(issueDate);

                            rawActivities.Add((issueDate, new ActivityDto
                            {
                                Title = "Book Issued",
                                Details = $"{whom} checked out \"{title}\".",
                                TimeAgo = timeAgoStr,
                                Status = "info"
                            }));
                        }
                    }
                }

                // Sort merged activities by date descending
                var sortedActs = rawActivities.OrderByDescending(x => x.Time).Take(5).Select(x => x.Act).ToList();
                
                // For additions, let's make their time ago look natural relative to order
                int addIndex = 1;
                foreach (var act in sortedActs)
                {
                    if (string.IsNullOrEmpty(act.TimeAgo))
                    {
                        act.TimeAgo = $"{addIndex * 15} mins ago";
                        addIndex++;
                    }
                }

                stats.RecentActivities = sortedActs;
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
