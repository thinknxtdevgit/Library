using System;

namespace lib.DtoModel.SearchAccessionDto
{
    public class AccessionIssueDetailDto
    {
        public DateTime? IssueDate { get; set; }
        public string? AccessionNo { get; set; }
        public long? IDNo { get; set; }
        public string? WhomIssued { get; set; }
        public string? Discipline { get; set; }
        public DateTime? LastReturnDate { get; set; }
        public string? Remarks { get; set; }
        public string? ClassRollNo { get; set; }
        public string? UniRollNo { get; set; }
    }
}
