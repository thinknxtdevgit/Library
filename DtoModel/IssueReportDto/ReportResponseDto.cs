namespace lib.DtoModel.IssueReportDto
{
    public class ReportResponseDto
    {
        public int TotalBooks { get; set; }
        public int IssuedBooks { get; set; }
        public int UnissuedBooks { get; set; }
        public List<IssueBookReportDto> IssueBooks { get; set; } = new();
    }
}
