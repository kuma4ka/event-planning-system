using EventPlanning.Application.Interfaces;
using EventPlanning.Application.DTOs.Guest;

namespace EventPlanning.Application.Validators.Guest;

public class CreateGuestDtoValidator(ICountryService countryService) : GuestBaseDtoValidator<CreateGuestDto>(countryService);