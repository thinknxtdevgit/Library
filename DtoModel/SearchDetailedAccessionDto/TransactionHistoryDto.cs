namespace lib.DtoModel.SearchDetailedAccessionDto
{
    public class TransactionHistoryDto
    {
        public string TransactionDate { get; set; }

        public string TransactionTime { get; set; }

        public string TransactionName { get; set; }

        public string Title { get; set; }

        public string IDNo { get; set; }

        public string PersonName { get; set; }

        public string PersonType { get; set; }

        public DateTime? RenewalDate { get; set; }

        public string UserID { get; set; }
    }
}
