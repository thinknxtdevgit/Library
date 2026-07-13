using lib.DtoModel.StaffReportDto;

namespace lib.Interface
{
    public interface IStaffReportService
    {
        Task<StaffReportResponseDto> SearchAsync(StaffReportRequestDto request);
    }
}
