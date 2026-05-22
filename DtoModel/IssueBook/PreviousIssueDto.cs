namespace lib.DtoModel.IssueBook
{
    public class PreviousIssueDto
    {
  
        public string IssueDate { get; set; }

        public string IssueTime { get; set; }

        public string IDNo { get; set; }

        public string Title { get; set; }

        public string Author { get; set; }

        public string AccessionNo { get; set; }

        public string LastReturnDate { get; set; }

        public string Course { get; set; }

        public string Remarks { get; set; }

        public string WhomIssued { get; set; }

        public string Type { get; set; }

        public int Days { get; set; }

        public decimal Fine { get; set; }

        public bool IsOverDue { get; set; }
    }
}
