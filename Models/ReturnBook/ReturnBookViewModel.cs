namespace lib.Models.ReturnBook
{
    public class ReturnBookViewModel
    {
        public ReceiveBookRequest Request { get; set; }

        public BookIssueDetailResponse BookDetail { get; set; }

        public ReceiveSuccessResponse ReceiveResponse { get; set; }
    }
}
