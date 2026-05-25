using lib.DtoModel.IssueReportDto;

namespace lib.Interface
{
    public interface IReportService
    {
        Task<ReportResponseDto> GetCollegeReportAsync(string collegeName);
        Task<byte[]> ExportCollegeReportAsync(string collegeName);
    }
}
