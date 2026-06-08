namespace lib.DtoModel.IssueBook
{
    public class IssueBookResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public UserDetailDto UserDetail { get; set; }

        public BookDetailDto BookDetail { get; set; }

        public List<PreviousIssueDto> PreviousIssues { get; set; }

        public int TotalIssuedBooks { get; set; }
        public decimal TotalFine { get; set; }
        public int IssueLimit { get; set; }
    }
}
