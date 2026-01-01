# Architectural Refactoring Plan

## Goal Description
Address architectural violations identified in the analysis report. Specifically, we will improve separation of concerns by moving domain logic back to the Domain layer, extracting caching logic from the Service layer into a Decorator, and removing loosely-typed `ViewBag` usage in the Web layer in favor of strong ViewModels.

## Proposed Changes

### Domain Layer
#### [MODIFY] [PhoneNumber.cs](file:///e:/PetProjects/event-planning-system/src/EventPlanning.Domain/ValueObjects/PhoneNumber.cs)
- Implement `Parse` and `Format` logic that currently resides in `EventService`.

### Application Layer
#### [NEW] [CachedEventService.cs](file:///e:/PetProjects/event-planning-system/src/EventPlanning.Application/Services/CachedEventService.cs)
- Create a Decorator for `IEventService` that handles caching for `GetEventDetailsAsync`.

#### [MODIFY] [EventService.cs](file:///e:/PetProjects/event-planning-system/src/EventPlanning.Application/Services/EventService.cs)
- Remove `ParsePhoneNumber` static method.
- Remove `ICacheService` dependency and all caching logic.
- Simplify `GetEventDetailsAsync` to strictly fetch data and map DTOs.

#### [MODIFY] [DependencyInjection.cs](file:///e:/PetProjects/event-planning-system/src/EventPlanning.Application/DependencyInjection.cs)
- Update DI registration to decorate `EventService` with `CachedEventService`.

### Web (MVC) Layer
#### [NEW] [EventListViewModel.cs](file:///e:/PetProjects/event-planning-system/src/EventPlanning.Web/Models/EventListViewModel.cs)
- Create a strongly-typed ViewModel to hold search results, pagination, and current filter state (replacing `ViewBag`).

#### [MODIFY] [EventController.cs](file:///e:/PetProjects/event-planning-system/src/EventPlanning.Web/Controllers/EventController.cs)
- Refactor `MyEvents` action to return `EventListViewModel`.
- Remove `SetMyEventsViewBag`.

#### [MODIFY] [MyEvents.cshtml](file:///e:/PetProjects/event-planning-system/src/EventPlanning.Web/Views/Event/MyEvents.cshtml)
- Update view to use `model EventListViewModel`.

## Verification Plan
### Automated Tests
- Run existing unit tests to ensure no regressions in business logic.
- Verify `EventService` tests still pass (might need adjustments for removed caching dependencies).

### Manual Verification
- **Caching**: Verify Event Details page still loads and hits cache (logs/debug).
- **Phone Numbers**: Verify Guest List on Event Details correctly formats phone numbers.
- **My Events**: Verify Sorting, Filtering, and Pagination work correctly with the new ViewModel.
