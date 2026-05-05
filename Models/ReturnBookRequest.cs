namespace lib.Models
{
    public class ReturnBookRequest
    {
        public string CollegeName { get; set; }

        public long AccessionNo { get; set; }   // ✅ change
        public long IDNo { get; set; }          // ✅ change

        public string Title { get; set; }
        public string Author { get; set; }
        public string PersonName { get; set; }
        public string Type { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime LastReturnDate { get; set; }

        public string Signature { get; set; }
    }
}
