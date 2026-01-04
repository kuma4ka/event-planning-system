namespace EventPlanning.Domain.Models;

public class PagedList<T>(List<T> items, int count, int pageNumber, int pageSize)
{
    public List<T> Items { get; private set; } = items;
    public int PageNumber { get; private set; } = pageNumber;
    private int TotalPages { get; set; } = (int)Math.Ceiling(count / (double)pageSize);
    public int TotalCount { get; private set; } = count;

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public int PageSize { get; private set; } = pageSize;

    public static PagedList<T> Create(List<T> source, int count, int pageNumber, int pageSize)
    {
        return new PagedList<T>(source, count, pageNumber, pageSize);
    }
}