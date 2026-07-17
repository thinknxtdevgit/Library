using System;

namespace lib.DtoModel.SearchIssueDatesDto
{
    public class IssueDatesSearchRequestDto
    {
        public string? CollegeName { get; set; }
        public DateTime? IssueDateFrom { get; set; }
        public DateTime? IssueDateTo { get; set; }
    }
}
