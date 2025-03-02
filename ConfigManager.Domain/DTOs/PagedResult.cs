
namespace ConfigManager.Domain.DTOs
{
public class PagedResult<T>
{
    public IEnumerable<T> Items { get; set; } = new List<T>();
    public int TotalPages { get; set; }
    public int TotalItems { get; set; }
}

}
