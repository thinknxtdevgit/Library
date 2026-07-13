using lib.DtoModel.MasterIssueLimitDto;

namespace lib.Interface
{
    public interface IMasterIssueLimitService
    {
        Task<MasterIssueLimitResponseDto> GetIssueLimitAsync(string collegeName, string personType);

        Task<MasterIssueLimitResponseDto> AddIssueLimitAsync(MasterIssueLimitDto dto);

        Task<MasterIssueLimitResponseDto> UpdateIssueLimitAsync(MasterIssueLimitDto dto);
    }
}
