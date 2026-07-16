using lib.DtoModel.StudentReportDto;

namespace lib.Interface
{
    public interface IStudentReportService
    {
        Task<List<CollegeDto>> GetCollegesAsync();

        Task<List<CourseDto>> GetCoursesAsync(string collegeName);

        Task<List<BatchDto>> GetBatchAsync(string collegeName, string course);

        Task<StudentReportResponseDto> SearchAsync(StudentReportRequestDto request);
    }
}
