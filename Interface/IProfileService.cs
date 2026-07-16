using lib.DtoModel.UserProfileDto;

namespace lib.Interface
{
    public interface IProfileService
    {
        Task<UserProfileDto> GetProfileAsync(string userName);
    }
}
