using System.Threading.Tasks;
using lib.DtoModel.SearchIssueDatesDto;

namespace lib.Interface
{
    public interface ISearchIssueDatesService
    {
        Task<IssueDatesSearchResponseDto> SearchIssueDatesAsync(IssueDatesSearchRequestDto request);
        Task<byte[]> ExportIssueDatesExcelAsync(IssueDatesSearchRequestDto request);
    }
}
