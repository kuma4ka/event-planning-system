namespace EventPlanning.Application.Constants;

public record CountryInfo(string Code, string Flag, string Name)
{
    public string DisplayValue => $"{Flag} {Code}";
}

public static class CountryConstants
{
    public static readonly List<CountryInfo> SupportedCountries =
    [
        // --- Priority / Default ---
        new("+380", "🇺🇦", "Ukraine"),
        
        // --- North America ---
        new("+1",   "🇺🇸", "USA"),
        new("+1",   "🇨🇦", "Canada"),
        new("+52",  "🇲🇽", "Mexico"),

        // --- Western & Central Europe ---
        new("+44",  "🇬🇧", "UK"),
        new("+49",  "🇩🇪", "Germany"),
        new("+33",  "🇫🇷", "France"),
        new("+31",  "🇳🇱", "Netherlands"),
        new("+32",  "🇧🇪", "Belgium"),
        new("+41",  "🇨🇭", "Switzerland"),
        new("+43",  "🇦🇹", "Austria"),
        new("+353", "🇮🇪", "Ireland"),
        
        // --- Southern Europe ---
        new("+39",  "🇮🇹", "Italy"),
        new("+34",  "🇪🇸", "Spain"),
        new("+351", "🇵🇹", "Portugal"),
        new("+30",  "🇬🇷", "Greece"),
        new("+356", "🇲🇹", "Malta"),
        new("+357", "🇨🇾", "Cyprus"),

        // --- Eastern Europe & Baltics ---
        new("+48",  "🇵🇱", "Poland"),
        new("+420", "🇨🇿", "Czechia"),
        new("+421", "🇸🇰", "Slovakia"),
        new("+36",  "🇭🇺", "Hungary"),
        new("+40",  "🇷🇴", "Romania"),
        new("+359", "🇧🇬", "Bulgaria"),
        new("+370", "🇱🇹", "Lithuania"),
        new("+371", "🇱🇻", "Latvia"),
        new("+372", "🇪🇪", "Estonia"),
        new("+373", "🇲🇩", "Moldova"),
        new("+385", "🇭🇷", "Croatia"),
        new("+386", "🇸🇮", "Slovenia"),
        
        // --- Nordics ---
        new("+46",  "🇸🇪", "Sweden"),
        new("+47",  "🇳🇴", "Norway"),
        new("+45",  "🇩🇰", "Denmark"),
        new("+358", "🇫🇮", "Finland"),
        new("+354", "🇮🇸", "Iceland"),

        // --- Asia & Pacific ---
        new("+90",  "🇹🇷", "Turkey"),
        new("+995", "🇬🇪", "Georgia"),
        new("+81",  "🇯🇵", "Japan"),
        new("+82",  "🇰🇷", "South Korea"),
        new("+86",  "🇨🇳", "China"),
        new("+91",  "🇮🇳", "India"),
        new("+61",  "🇦🇺", "Australia"),
        new("+64",  "🇳🇿", "New Zealand"),
        new("+65",  "🇸🇬", "Singapore"),
        new("+66",  "🇹🇭", "Thailand"),
        new("+84",  "🇻🇳", "Vietnam"),
        new("+62",  "🇮🇩", "Indonesia"),

        // --- Middle East ---
        new("+972", "🇮🇱", "Israel"),
        new("+971", "🇦🇪", "UAE"),
        new("+966", "🇸🇦", "Saudi Arabia"),

        // --- South America & Africa ---
        new("+55",  "🇧🇷", "Brazil"),
        new("+54",  "🇦🇷", "Argentina"),
        new("+56",  "🇨🇱", "Chile"),
        new("+57",  "🇨🇴", "Colombia"),
        new("+27",  "🇿🇦", "South Africa"),
        new("+20",  "🇪🇬", "Egypt")
    ];

    public const string DefaultCode = "+380";
}