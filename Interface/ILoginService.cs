using lib.DtoModel.LoginDto;

namespace lib.Interface
{
    public interface ILoginService
    {
        Task<List<string>> GetLoginTypesAsync();

        Task<LoginResponse> LoginAsync(LoginRequest model);

        Task<List<MenuNode>> GetDynamicMenuAsync();
    }
}
