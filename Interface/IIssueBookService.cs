using lib.DtoModel.IssueBook;

namespace lib.Interface
{
    public interface IIssueBookService
    {
        Task<IssueBookResponseDto> CheckIdAsync(IssueBookRequestDto request);
    }
}
