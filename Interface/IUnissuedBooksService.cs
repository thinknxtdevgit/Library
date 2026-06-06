using lib.DtoModel.ReferenceBookDto;
using lib.DtoModel.UnissuedBookDto;
using lib.Pagination_Helper;

namespace lib.Interface
{
    public interface IUnissuedBooksService
    {
        Task<List<UnissuedBookDto>> GetUnissuedBooksAsync(string collegeName);
        Task<PagedResult<UnissuedBookDto>> GetUnissuedBooksAsyncPages(string collegeName, int pageNumber, int pageSize);
        //  byte[] ExportToExcel(List<UnissuedBookDto> data);
    }
}
