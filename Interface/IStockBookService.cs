using lib.DtoModel.StockBookDto;

namespace lib.Interface
{
    public interface IStockBookService
    {
        Task<List<StockBookDto>> GetStockBooksAsync(string collegeName);
        Task<int> GetTotalBooksAsync(string collegeName);
        Task<int> GetTotalTitlesAsync(string collegeName);
        Task<int> GetUnusedBooksAsync(string collegeName);
        Task<byte[]> ExportToExcelAsync(string collegeName);
    }
}
