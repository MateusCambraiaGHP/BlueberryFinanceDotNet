namespace BlueBerryFinance.Common.ViewModels
{
    public class PaginationMeta
    {
        public int TotalCount { get; private set; }
        public int Page { get; private set; }
        public int PageSize { get; private set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
        public bool HasNext => Page < TotalPages;
        public bool HasPrev => Page > 1;

        public PaginationMeta(int totalCount, int page, int pageSize)
        {
            TotalCount = totalCount;
            Page = page;
            PageSize = pageSize;
        }
    }
}
