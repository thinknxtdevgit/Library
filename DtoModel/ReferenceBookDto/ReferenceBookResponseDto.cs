namespace lib.DtoModel.ReferenceBookDto
{
    public class ReferenceBookResponseDto
    {
        public int TotalRecords { get; set; }

        public List<ReferenceBookDto> Books { get; set; } = new();

    }
}
