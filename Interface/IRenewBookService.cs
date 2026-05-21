using lib.DtoModel.RenewBookDto;

namespace lib.Interface
{
    public interface IRenewBookService
    {
        Task<RenewBookResponseDto> RenewBookAsync(RenewBookRequestDto request);



    }
}
