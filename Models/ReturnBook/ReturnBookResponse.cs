namespace lib.Models.ReturnBook
{
    public class ReturnBookResponse
    {
        public bool Success { get; set; }

        public string Mode { get; set; }

        public string Message { get; set; }

        public BookIssueDetailResponse Data { get; set; }
    }
}
