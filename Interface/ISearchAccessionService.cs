using lib.DtoModel.SearchAccessionDto;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace lib.Interface
{
    public interface ISearchAccessionService
    {
        Task<List<string>> GetPublishersAsync(string collegeName);
        Task<List<string>> GetCategoriesAsync(string collegeName);
        Task<List<string>> GetSourcesAsync(string collegeName);
        Task<AccessionSearchResponse?> SearchAccessionAsync(string collegeName, string accessionNo);
        Task<byte[]?> GetStudentImageAsync(string collegeName, long idNo);
        Task<bool> UpdateStockAsync(AccessionUpdateRequest request);
        Task<bool> UpdateIssueRemarksAsync(string collegeName, string accessionNo, long idNo, string remarks);
    }
}
