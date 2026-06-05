using lib.DtoModel.ReferenceBookDto;

namespace lib.Interface
{
    public interface IReferenceBookService
    {
        Task<ReferenceBookResponseDto>GetReferenceBooksAsync(string collegeName);

    }
}
