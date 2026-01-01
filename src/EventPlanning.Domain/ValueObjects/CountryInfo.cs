namespace EventPlanning.Domain.ValueObjects;

public record CountryInfo(string Code, string Flag, string Name)
{
    public string DisplayValue => $"{Flag} {Code}";
}
