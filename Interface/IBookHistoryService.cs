using lib.DtoModel.BookHistoryDto;

namespace lib.Interface
{
    public interface IBookHistoryService
    {
        Task<List<BookHistoryDto>> GetBookHistoryAsync(string collegeName, string accessionNo);
    }
}
