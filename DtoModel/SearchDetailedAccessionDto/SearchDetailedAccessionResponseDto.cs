namespace lib.DtoModel.SearchDetailedAccessionDto
{
    public class SearchDetailedAccessionResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; }

        public List<TransactionHistoryDto> Transactions { get; set; } = new List<TransactionHistoryDto>();

    }
}
