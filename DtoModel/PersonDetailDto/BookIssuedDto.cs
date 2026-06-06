namespace lib.DtoModel.PersonDetailDto
{
    public class BookIssuedDto
    {
        public string AccessionNo { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime? IssueDate { get; set; }
        public DateTime? LastReturnDate { get; set; }
        public string Condition { get; set; }
        public string Remarks { get; set; }
    }
}
