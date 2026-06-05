using lib.DtoModel.IssueBook;

namespace lib.Interface
{
    public interface IIssueBookService
    {
        Task<IssueBookResponseDto> CheckIdAsync(IssueBookRequestDto request);
        Task<IssueBookResponseDto> IssueBookAsync(IssueBookRequestDto request);
        Task<IssueBookResponseDto> CheckAccessionDetailAsync(
    string accessionNo,
    string collegeName);
    }
}
