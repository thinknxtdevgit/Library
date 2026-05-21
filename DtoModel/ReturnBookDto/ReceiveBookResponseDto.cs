namespace lib.DtoModel.ReturnBookDto
{
    public class ReceiveBookResponseDto
    {
        public bool Success { get; set; }

        public string? Mode { get; set; }

        public string? Message { get; set; }

        public ReceiveBookDataDto? Data { get; set; }
    }
}
