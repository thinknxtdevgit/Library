using lib.DtoModel.SearchStudentNameDto;

namespace lib.Interface
{
    public interface ISearchStudentNameService
    {
        Task<List<string>> GetCollegesAsync();
        Task<StudentSearchResponseDto> SearchStudentAsync(string collegeName, string studentName);
        Task<byte[]> ExportStudentExcelAsync(string collegeName, string studentName);






    }
}
