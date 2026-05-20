namespace lib.Models.ReturnBook
{
    public class BookIssueDetailResponse
    {
        public string Name { get; set; }

        public long IdNo { get; set; }

        public string Title { get; set; }

        public string Type { get; set; }

        public string Author { get; set; }

        public DateTime IssueDate { get; set; }

        public DateTime LastReturnDate { get; set; }

        public string College { get; set; }

        public ExtraDetailResponse ExtraDetail { get; set; }

        public string Snap { get; set; }
    }
}
