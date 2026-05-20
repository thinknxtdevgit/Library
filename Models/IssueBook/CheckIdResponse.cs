namespace lib.Models.IssueBook
{
    public class CheckIdResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string Mode { get; set; }

        public UserDetailResponse Data { get; set; }

        public PreviousIssueResponse PreviousIssue { get; set; }

        public int TotalIssuedBooks { get; set; }

        public BookDetailResponse AccessionDetail { get; set; }
    }
}
