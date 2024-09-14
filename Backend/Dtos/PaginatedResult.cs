namespace Backend.Dtos;

public class PaginatedResult<T>
{
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public IEnumerable<T> Items { get; set; }
}