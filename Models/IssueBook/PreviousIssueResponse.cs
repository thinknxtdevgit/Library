namespace lib.Models.IssueBook
{
    public class PreviousIssueResponse
    {
        public int Total { get; set; }

        public List<PreviousIssueItem> Data { get; set; }
    }
}
