using EventPlanning.Application.Interfaces;
using EventPlanning.Domain.ValueObjects;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;
using System.Text.Json;

namespace EventPlanning.Infrastructure.Services;

public class CountryService : ICountryService
{
    private readonly IEnumerable<CountryInfo> _supportedCountries;
    private const string DefaultCode = "+380";
    private const string CacheKey = "SupportedCountries";

    public CountryService(IConfiguration configuration, IWebHostEnvironment env, ILogger<CountryService> logger, IMemoryCache cache)
    {
        _supportedCountries = cache.GetOrCreate(CacheKey, entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24);
            return LoadCountries(configuration, env, logger);
        }) ?? new List<CountryInfo>();
    }

    private List<CountryInfo> LoadCountries(IConfiguration configuration, IWebHostEnvironment env, ILogger<CountryService> logger)
    {
        var filePath = configuration["CountrySettings:FilePath"];
        if (string.IsNullOrEmpty(filePath))
        {
            filePath = "countries.json";
        }

        var fullPath = Path.Combine(env.ContentRootPath, filePath);

        if (!File.Exists(fullPath))
        {
             logger.LogWarning("Country data file not found at {Path}. Using empty list.", fullPath);
             return new List<CountryInfo>();
        }

        try 
        {
             var json = File.ReadAllText(fullPath);
             return JsonSerializer.Deserialize<List<CountryInfo>>(json) ?? new List<CountryInfo>();
        }
        catch (Exception ex)
        {
             logger.LogError(ex, "Failed to load country data from {Path}", fullPath);
             return new List<CountryInfo>();
        }
    }

    public List<CountryInfo> GetSupportedCountries() => _supportedCountries.ToList();

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
