using lib.DtoModel.PersonDetailDto;

namespace lib.Interface
{
    public interface IPersonSearchService
    {
        Task<PersonSearchResponseDto> SearchPersonAsync(string idNo, bool isUniversityRollNo);


    }
}
