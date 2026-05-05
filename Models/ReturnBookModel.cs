namespace lib.Models
{
    public class ReturnBookModel
    {
        public string CollegeName { get; set; }
        public string Type { get; set; } // Student / Staff
        public string AccessionNo { get; set; }
        public string IDNo { get; set; }

        public string AuthorName { get; set; }

        public int FineDays { get; set; }
        public int TotalFine { get; set; }

        public DateTime IssueDate { get; set; }
        public DateTime LastReturnDate { get; set; }
        public DateTime ReturnDate { get; set; }

        public int DaysLate { get; set; }

    }
}
