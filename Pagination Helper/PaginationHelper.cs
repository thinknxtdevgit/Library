namespace lib.Pagination_Helper
{
    public class PaginationHelper
    {
        public static int GetOffset(int pageNumber, int pageSize)
        {
            return (pageNumber - 1) * pageSize;
        }
    }
}
