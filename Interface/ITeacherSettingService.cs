using lib.DtoModel.TeacherSettingDto;

namespace lib.Interface
{
    public interface ITeacherSettingService
    {
        Task<List<TeacherSettingDto>> GetTeachers(string collegeName);

        Task<int> GetTotalTeachers(string collegeName);

        Task<bool> AddTeacher(TeacherSettingDto dto);

        Task<bool> UpdateTeacher(string oldId, TeacherSettingDto dto);

        Task<byte[]> ExportExcelAsync(string collegeName);
    }
}
