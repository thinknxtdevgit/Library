using lib.DtoModel.SearchISBNDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lib.Interface
{
    public interface ISearchISBNService
    {
        Task<List<ISBNSearchDto>> SearchAsync(string collegeName, string isbn);
        Task<byte[]> ExportExcelAsync(string collegeName, string isbn);
    }
}
