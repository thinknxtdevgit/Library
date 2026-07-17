namespace lib.DtoModel.SearchAccessionDto
{
    public class AccessionDetailDto
    {
        public string? BindingBook { get; set; }
        public string? Title { get; set; }
        public string? FirstAuthorForeName { get; set; }
        public string? SecondAuthorForeName { get; set; }
        public string? ThirdAuthorForeName { get; set; }
        public string? FirstAuthorSirName { get; set; }
        public string? SecondAuthorSirName { get; set; }
        public string? ThirdAuthorSirName { get; set; }
        public string? Author { get; set; }
        public string? MoreThanThreeAuthors { get; set; } // Represented as "True"/"False" string in db
        public string? Publisher { get; set; }
        public string? Edition { get; set; }
        public string? Price { get; set; }
        public string? Discount { get; set; }
        public string? NetPrice { get; set; }
        public string? Year { get; set; }
        public string? Pages { get; set; }
        public string? BillNo { get; set; }
        public string? BillDate { get; set; }
        public string? Location { get; set; }
        public string? ClassNo { get; set; }
        public string? BookNo { get; set; }
        public string? SubTitle { get; set; }
        public string? ISBN { get; set; }
        public string? Place { get; set; }
        public string? BookSize { get; set; }
        public string? Series { get; set; }
        public string? Subject1 { get; set; }
        public string? Subject2 { get; set; }
        public string? Remarks { get; set; }
        public string? Type { get; set; }
        public string? Category { get; set; }
        public string? Source { get; set; }
        public string? CollegeName { get; set; }
        public string? AccessionNo { get; set; }
    }
}
