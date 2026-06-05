using lib.DtoModel.IssueReportDto;
using lib.DtoModel.StockBookDto;

namespace lib.Interface
{
    public interface IReportService
    {
        Task<List<IssueBookReportDto>> GetIssueBooksReport(string collegeName);
        Task<ReportResponseDto> GetCollegeReportAsync(string collegeName);
        Task<byte[]> ExportCollegeReportAsync(string collegeName);
    }
}
