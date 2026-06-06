namespace lib.DtoModel.PersonDetailDto
{
    public class PersonSearchResponseDto
    {

        public string CollegeName { get; set; }
        public string Name { get; set; }
        public string CourseOrDesignation { get; set; }
        public string BatchOrDepartment { get; set; }
        public string RollNoOrIdNo { get; set; }
        public string PersonType { get; set; }

        public byte[] Snap { get; set; }

        public List<BookIssuedDto> Books { get; set; } = new();
        public List<CDIssuedDto> CDs { get; set; } = new();
        public List<FineDetailDto> Fines { get; set; } = new();
    }
}
