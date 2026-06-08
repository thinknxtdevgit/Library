namespace lib.DtoModel.SearchPersonIdDto
{
    public class PersonTransactionResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public List<PersonTransactionDto> Data { get; set; } = new();
    }
}
