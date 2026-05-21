namespace lib.DtoModel.RenewBookDto
{
    public class RenewBookResponseDto
    {
        public bool Success { get; set; }

        public string? Message { get; set; }

        public string? Mode { get; set; }

        public RenewBookDetailDto? Data { get; set; }

        public string? Snap { get; set; }
    }
}
