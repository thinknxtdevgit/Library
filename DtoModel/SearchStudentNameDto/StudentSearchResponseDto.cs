namespace lib.DtoModel.SearchStudentNameDto
{
    public class StudentSearchResponseDto
    {
        public bool Success { get; set; }

        public string Message { get; set; } = string.Empty;

        public List<StudentDetailDto> Data { get; set; } = new();
    }
}
