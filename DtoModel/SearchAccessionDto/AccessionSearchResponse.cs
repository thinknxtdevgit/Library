namespace lib.DtoModel.SearchAccessionDto
{
    public class AccessionSearchResponse
    {
        public AccessionDetailDto Book { get; set; } = new AccessionDetailDto();
        public AccessionIssueDetailDto? Issue { get; set; }
    }
}
