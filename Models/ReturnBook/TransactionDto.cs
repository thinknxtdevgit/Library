namespace lib.Models.ReturnBook
{
    public class TransactionDto
    {
        public long Id { get; set; }

        public string CollegeName { get; set; }

        public DateTime TransactionDate { get; set; }

        public DateTime TransactionTime { get; set; }

        public string TransactionName { get; set; }

        public string Type { get; set; }

        public long AccessionNo { get; set; }

        public string Title { get; set; }

        public long IDNo { get; set; }

        public string PersonName { get; set; }

        public string PersonType { get; set; }

        public long UserID { get; set; }

        public string UserName { get; set; }
    }
}
