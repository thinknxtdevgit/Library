namespace lib.DtoModel.FacultySearchDto
{
    public class FacultySearchResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<FacultyDetailDto> Data { get; set; } = new();
    }
}
