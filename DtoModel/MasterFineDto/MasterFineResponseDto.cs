namespace lib.DtoModel.MasterFineDto
{
    public class MasterFineResponseDto
    {

        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public MasterFineDto? Data { get; set; }
    }
}
