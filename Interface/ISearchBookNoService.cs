using lib.DtoModel.SearchBookNoDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lib.Interface
{
    public interface ISearchBookNoService
    {
        Task<List<string>> GetAuthorizedCollegesAsync();
        Task<List<BookNoSearchDto>> SearchAsync(string collegeName, string bookNo);
        Task<byte[]> ExportExcelAsync(string collegeName, string bookNo);
    }
}
