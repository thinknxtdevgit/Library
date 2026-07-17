using lib.DtoModel.SearchClassNoDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lib.Interface
{
    public interface ISearchClassNoService
    {
        Task<List<ClassNoSearchDto>> SearchAsync(string collegeName, string classNo);
        Task<byte[]> ExportExcelAsync(string collegeName, string classNo);
    }
}
