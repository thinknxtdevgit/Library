namespace lib.Models.IssueBook
{
    public class BookDetailResponse
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public string AccessionNo { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string Category { get; set; }
    }
}
