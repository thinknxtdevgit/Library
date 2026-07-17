using System.Collections.Generic;

namespace lib.DtoModel.SearchIssueDatesDto
{
    public class IssueDatesSearchResponseDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public List<IssueDetailDto> Data { get; set; } = new();
    }
}
