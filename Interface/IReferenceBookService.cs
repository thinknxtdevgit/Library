using lib.DtoModel.ReferenceBookDto;
using lib.Pagination_Helper;


namespace lib.Interface
{
    public interface IReferenceBookService
    {
        Task<ReferenceBookResponseDto>GetReferenceBooksAsync(string collegeName);
        Task<PagedResult<ReferenceBookDto>> GetReferenceBooksAsyncPages(string collegeName, int pageNumber, int pageSize);
    }
}
