namespace lib.DtoModel.MasterIssueLimitDto
{
    public class MasterIssueLimitResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public MasterIssueLimitDto? Data { get; set; }
    }
}
