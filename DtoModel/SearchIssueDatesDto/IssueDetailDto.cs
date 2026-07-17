using System;

namespace lib.DtoModel.SearchIssueDatesDto
{
    public class IssueDetailDto
    {
        public DateTime? IssueDate { get; set; }
        public string? IDNo { get; set; }
        public string? Title { get; set; }
        public string? AccessionNo { get; set; }
        public string? WhomIssued { get; set; }
        public DateTime? LastDate { get; set; } // Maps to LastReturnDate as LastDate
        public string? Condition { get; set; }
        public string? Discipline { get; set; }
        public string? Type { get; set; }
        public string? Remarks { get; set; }
    }
}
