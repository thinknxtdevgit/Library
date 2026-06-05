using lib.DtoModel.UnissuedBookDto;

namespace lib.Interface
{
    public interface IUnissuedBooksService
    {
        Task<List<UnissuedBookDto>> GetUnissuedBooksAsync(string collegeName);
      //  byte[] ExportToExcel(List<UnissuedBookDto> data);
    }
}
