using lib.DtoModel.ReturnBookDto;

namespace lib.Interface
{
    public interface IReturnBookService
    {
        Task<ReceiveBookResponseDto> ReceiveBookAsync(ReceiveBookRequestDto request);

    }
}
