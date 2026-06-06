namespace lib.DtoModel.PersonDetailDto
{
    public class FineDetailDto
    {
        public string AccessionNo { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public DateTime? DateOfIssue { get; set; }
        public DateTime? LastReturnDate { get; set; }
        public DateTime? DateOfFine { get; set; }
        public decimal Fine { get; set; }
        public string FineStatus { get; set; }
    }
}
