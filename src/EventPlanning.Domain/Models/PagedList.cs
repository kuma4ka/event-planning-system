namespace EventPlanning.Domain.Models;

public class PagedList<T>
{
    public List<T> Items { get; private set; }
    public int PageNumber { get; private set; }
    public int TotalPages { get; private set; }
    public int TotalCount { get; private set; }

    public bool HasPreviousPage => PageNumber > 1;
    public bool HasNextPage => PageNumber < TotalPages;

    public PagedList(List<T> items, int count, int pageNumber, int pageSize)
    {
        TotalCount = count;
        PageSize = pageSize;
        PageNumber = pageNumber;
        TotalPages = (int)Math.Ceiling(count / (double)pageSize);
        Items = items;
    }

    public int PageSize { get; private set; }

    public static PagedList<T> Create(List<T> source, int count, int pageNumber, int pageSize)
    {
        return new PagedList<T>(source, count, pageNumber, pageSize);
    }
}