namespace lib.DtoModel.AddStockBookDto
{
    public class RequestDto
    {
        public string? Action { get; set; }
        public long? CollegeId { get; set; }
        public long? AccessionId { get; set; }
        public string? CollegeName { get; set; }

        public string? AccessionNo { get; set; }

        public DateTime? DateEntry { get; set; }

        public string? BillDate { get; set; }

        public string? Title { get; set; }

        public string? Author { get; set; }

        public string? FirstAuthorForename { get; set; }

        public string? FirstAuthorSirName { get; set; }

        public string? SecondAuthorForename { get; set; }

        public string? SecondAuthorSurname { get; set; }

        public string? ThirdAuthorForename { get; set; }

        public string? ThirdAuthorSurname { get; set; }

        public string? MoreThanThreeAuthors { get; set; }

        public string? Publisher { get; set; }

        public string? Source { get; set; }

        public string? Edition { get; set; }

        public int? Quantity { get; set; }

        public decimal? Price { get; set; }

        public decimal? Discount { get; set; }

        public string? Type { get; set; }

        public string? Category { get; set; }

        public int? Year { get; set; }

        public int? Pages { get; set; }

        public string? BillNo { get; set; }

        public string? ClassNo { get; set; }

        public string? BookNo { get; set; }

        public string? Subtitle { get; set; }

        public string? ISBN { get; set; }

        public string? Size { get; set; }

        public string? Place { get; set; }

        public string? Location { get; set; }

        public decimal? NetPrice { get; set; }

        public string? Subject1 { get; set; }

        public string? Subject2 { get; set; }

        public string? Series { get; set; }

        public string? Remarks { get; set; }

        public string? SearchText { get; set; }

        public string? Field { get; set; }

        public string? Volume { get; set; }
    }
}
