namespace lib.Models
{
    public class AddStockRequest
    {
        public string CollegeName { get; set; }
        public DateTime DateEntry { get; set; }
        public int AccessionNo { get; set; }
        public string Author { get; set; }
        public string Title { get; set; }
        public string Edition { get; set; }
        public string Publisher { get; set; }
        public string Year { get; set; }
        public string Pages { get; set; }
        public string Source { get; set; }
        public decimal Price { get; set; }
        public decimal? Discount { get; set; }
        public decimal NetPrice { get; set; }
        public string Type { get; set; }
        public string Category { get; set; }
        public string BillNo { get; set; }
        public DateTime BillDate { get; set; }
        public string ClassNo { get; set; }
        public string BookNo { get; set; }
        public string Remarks { get; set; }
        public string Location { get; set; }
        public string FirstAuthorForeName { get; set; }
        public string FirstAuthorSirName { get; set; }
        public int Quantity { get; set; }
    }
}
