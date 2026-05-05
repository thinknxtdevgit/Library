namespace lib.Models
{
    public class BookIssueRequest
    {
        public string CollegeName { get; set; }
        public string IdNo { get; set; }
        public string AccessionNo { get; set; }
        public string WhomIssued { get; set; }
        public string Category { get; set; }
        public string Remarks { get; set; }
        public string Signature { get; set; }
        public string Course { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime IssueDate { get; set; }
        public DateTime LastReturnDate { get; set; }
        public string Type { get; set; } // Student / Staff
    }
}
