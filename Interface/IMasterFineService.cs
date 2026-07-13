using lib.DtoModel.MasterFineDto;

namespace lib.Interface
{
    public interface IMasterFineService
    {
        Task<MasterFineResponseDto> GetFineAsync(string collegeName);

        Task<MasterFineResponseDto> AddFineAsync(MasterFineDto dto);

        Task<MasterFineResponseDto> UpdateFineAsync(MasterFineDto dto);
    }
}
