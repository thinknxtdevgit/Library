using lib.DtoModel.SearchPersonIdDto;

namespace lib.Interface
{
    public interface ISearchPersonIdService
    {
        Task<List<string>> GetCollegesAsync();
        Task<PersonTransactionResponseDto> SearchAsync(string collegeName, string personId);
    }
}
