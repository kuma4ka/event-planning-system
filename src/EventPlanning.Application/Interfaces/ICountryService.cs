using EventPlanning.Domain.Constants;

namespace EventPlanning.Application.Interfaces;

public interface ICountryService
{
    List<CountryInfo> GetSupportedCountries();
    (string CountryCode, string LocalNumber) ParsePhoneNumber(string? fullPhoneNumber);
}
