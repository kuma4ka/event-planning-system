using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.Constants;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EventPlanning.Infrastructure.Services;

public class CountryService : ICountryService
{
    private readonly List<CountryInfo> _supportedCountries;
    private const string DefaultCode = "+380";

    public CountryService(IConfiguration configuration, ILogger<CountryService> logger)
    {
        var countrySection = configuration.GetSection("CountrySettings:SupportedCountries");
        var countries = countrySection.Get<List<CountryInfo>>();

        if (countries == null || countries.Count == 0)
        {
            logger.LogWarning("No supported countries found in configuration. Using defaults from CountryConstants.");
            _supportedCountries = CountryConstants.SupportedCountries;
        }
        else
        {
            _supportedCountries = countries;
        }
    }

    public List<CountryInfo> GetSupportedCountries() => _supportedCountries;

    public (string CountryCode, string LocalNumber) ParsePhoneNumber(string? fullPhoneNumber)
    {
        if (string.IsNullOrEmpty(fullPhoneNumber))
            return (DefaultCode, string.Empty);

        var country = _supportedCountries
            .OrderByDescending(c => c.Code.Length)
            .FirstOrDefault(c => fullPhoneNumber.StartsWith(c.Code));

        if (country != null)
        {
            var localNumber = fullPhoneNumber.Substring(country.Code.Length);
            return (country.Code, localNumber);
        }

        return (DefaultCode, fullPhoneNumber);
    }
}
