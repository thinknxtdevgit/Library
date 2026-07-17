using System.Collections.Generic;

namespace lib.DtoModel.DashboardDto
{
    public class TrendPointDto
    {
        public string Date { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class ActivityDto
    {
        public string Title { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string TimeAgo { get; set; } = string.Empty;
        public string Status { get; set; } = "info"; // success, info, danger, warning
    }

    public class DashboardStatsDto
    {
        public int TotalBooks { get; set; }
        public int IssuedBooks { get; set; }
        public int AvailableBooks { get; set; }
        public int OverdueBooks { get; set; }
        public int TotalStudents { get; set; }
        public int TotalStaff { get; set; }
        public int TotalCategories { get; set; }
        public int TotalPublishers { get; set; }
        public List<TrendPointDto> BorrowingTrends { get; set; } = new List<TrendPointDto>();
        public List<ActivityDto> RecentActivities { get; set; } = new List<ActivityDto>();
    }

    public class QuickSearchResultDto
    {
        public string Type { get; set; } = string.Empty; // "Book" or "Student"
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Subtitle { get; set; } = string.Empty;
    }
}
