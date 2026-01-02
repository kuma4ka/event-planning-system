using EventPlanning.Domain.Entities;
using EventPlanning.Domain.Enums;

namespace EventPlanning.Infrastructure.Specifications;

public class EventFilterSpecification : BaseSpecification<Event>
{
    public EventFilterSpecification(
        Guid? organizerId,
        Guid? viewerId,
        string? searchTerm,
        DateTime? from,
        DateTime? to,
        EventType? type,
        string? sortOrder,
        int pageNumber,
        int pageSize)
        : base(e =>
            (!viewerId.HasValue ? !e.IsPrivate : (!e.IsPrivate || e.OrganizerId == viewerId)) &&
            (!organizerId.HasValue || e.OrganizerId == organizerId) &&
            (string.IsNullOrWhiteSpace(searchTerm) || e.Name.Contains(searchTerm) || (e.Description != null && e.Description.Contains(searchTerm))) &&
            (!from.HasValue || e.Date >= from) &&
            (!to.HasValue || e.Date <= to) &&
            (!type.HasValue || e.Type == type)
        )
    {
        AddInclude(e => e.Venue!);

        ApplyPaging((pageNumber - 1) * pageSize, pageSize);

        switch (sortOrder)
        {
            case "name_asc":
                AddOrderBy(e => e.Name);
                break;
            case "name_desc":
                AddOrderByDescending(e => e.Name);
                break;
            case "date_asc":
                AddOrderBy(e => e.Date);
                break;
            case "newest":
                AddOrderByDescending(e => e.CreatedAt);
                break;
            default:
                AddOrderByDescending(e => e.Date);
                break;
        }
    }
}
