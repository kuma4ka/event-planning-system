using EventPlanning.Application.Interfaces;
using EventPlanning.Application.DTOs.Guest;

namespace EventPlanning.Application.Validators.Guest;

public class AddGuestManuallyDtoValidator(ICountryService countryService) : GuestBaseDtoValidator<AddGuestManuallyDto>(countryService);