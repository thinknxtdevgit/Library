namespace lib.DtoModel.StockBookDto
{
    public class StockBookDto
    {
        public DateTime ?DateEntry { get; set; }
        public long AccessionNo { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string Edition { get; set; }
        public string Publisher { get; set; }
        public string Year { get; set; }
        public string Pages { get; set; }
        public string Volume { get; set; }
        public string Source { get; set; }
        public string Price { get; set; }
        public decimal NetPrice { get; set; }
        public decimal Discount { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string BillNo { get; set; }
        public string ClassNo { get; set; }
        public string? BillDate { get; set; }
        public string BookNo { get; set; }
        public string Remarks { get; set; }
        public string Location { get; set; }

        public string FirstName { get; set; }
        public string SirName { get; set; }

        public string FirstAuthorForeName { get; set; }
        public string FirstAuthorSirName { get; set; }
        public string SecondAuthorForeName { get; set; }
        public string SecondAuthorSirName { get; set; }
        public string ThirdAuthorForeName { get; set; }
        public string ThirdAuthorSirName { get; set; }
        public string MoreThanThreeAuthors { get; set; }

        public string SubTitle { get; set; }
        public string ISBN { get; set; }
        public string Place { get; set; }
        public string Series { get; set; }
        public string BookSize { get; set; }
        public string Subject1 { get; set; }
        public string Subject2 { get; set; }
    }
}
