using lib.DtoModel.AddStockBookDto;

namespace lib.Interface
{
    public interface IStockRegisterService
    {
        object GetInitialData(string collegeName); 
        Dictionary<string, object> GetByAccession(string collegeName, string accessionNo);
        Dictionary<string, object> GetBookDetail(string collegeName, string title); 
        List<string> AutoComplete(string collegeName, string field, string search); 
        Task<string> AddBookAsync(RequestDto req);
        Task<string> UpdateBookAsync(RequestDto req);
    }
}
