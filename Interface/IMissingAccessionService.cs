using System.Collections.Generic;
using System.Threading.Tasks;

namespace lib.Interface
{
    public interface IMissingAccessionService
    {
        Task<List<string>> GenerateAndFindMissingAsync(string collegeName);
        Task<byte[]> ExportExcelAsync(string collegeName);
    }
}
