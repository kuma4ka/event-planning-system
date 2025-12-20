using EventPlanning.Domain.Enums;

namespace EventPlanning.Application.DTOs;

public record EventSearchDto
{
    public string? SearchTerm { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public EventType? Type { get; init; }
    
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 9;
}