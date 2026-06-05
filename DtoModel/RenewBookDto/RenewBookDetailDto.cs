namespace lib.DtoModel.RenewBookDto
{
    public class RenewBookDetailDto
    {
        public string? CollegeName { get; set; }

        public string? AccessionNo { get; set; }

        public string? Name { get; set; }

        public string? IDNo { get; set; }

        public string? Title { get; set; }

        public DateTime DateOfIssue { get; set; }

        public DateTime LastReturnDate { get; set; }

        public string? Type { get; set; }

        public string? Author { get; set; }

        public string? Discipline { get; set; }
    }
}
