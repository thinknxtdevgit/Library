namespace lib.DtoModel.StaffReportDto
{
    public class StaffReportResponseDto
    {
        public string CollegeName { get; set; }

        public string Address1 { get; set; }

        public string Address2 { get; set; }

        public int TotalRecords { get; set; }

        public List<StaffReportDto> StaffList { get; set; } = new();
    }
}
