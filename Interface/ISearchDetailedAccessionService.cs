using lib.DtoModel.SearchDetailedAccessionDto;

namespace lib.Interface
{
    public interface ISearchDetailedAccessionService
    {
        Task<SearchDetailedAccessionResponseDto> SearchAccessionNoAsync(SearchDetailedAccessionRequestDto request);
        Task<byte[]> ExportAccessionHistoryAsync(SearchDetailedAccessionRequestDto request);


    }
}
