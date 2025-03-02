namespace ConfigManager.Domain.DTOs
{
    public class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = new List<T>();
        public int TotalPages { get; init; }
        public int TotalItems { get; init; }
        public int PageNumber { get; init; }
        public int PageSize { get; init; }

        public PagedResult(IEnumerable<T> items, int totalItems, int pageNumber, int pageSize)
        {
            Items = items.ToList();
            TotalItems = totalItems;
            PageSize = pageSize;
            PageNumber = pageNumber;
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        }
    }
}
