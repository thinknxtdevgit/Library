using lib.DtoModel.FacultySearchDto;

namespace lib.Interface
{
    public interface IFacultySearchService
    {
        Task<FacultySearchResponseDto> SearchFacultyAsync(FacultySearchRequestDto request);
        Task<byte[]> ExportFacultyExcelAsync(FacultySearchRequestDto request);
    }
}
