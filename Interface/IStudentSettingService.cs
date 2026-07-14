using lib.DtoModel.StudentSettingDto;

namespace lib.Interface
{
    public interface IStudentSettingService
    {
        Task<List<StudentSettingDto>> GetStudents(string collegeName);

        Task<bool> AddStudent(StudentSettingDto dto);

        Task<bool> UpdateStudent(int oldId, StudentSettingDto dto);

        Task<byte[]> ExportExcelAsync(string collegeName);
    }
}
