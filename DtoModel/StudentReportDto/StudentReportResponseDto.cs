namespace lib.DtoModel.StudentReportDto
{
    public class StudentReportResponseDto
    {
        public string CollegeName { get; set; } = "";

        public string Address1 { get; set; } = "";

        public string Address2 { get; set; } = "";

        public int TotalRecords { get; set; }

        public List<StudentReportDto> StudentList { get; set; } = new();
    }
}
