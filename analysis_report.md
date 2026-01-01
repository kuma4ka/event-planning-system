# Solution Architecture Analysis

## Executive Summary
The solution follows a standard **Clean Architecture** approach with four distinct layers: `Domain`, `Application`, `Infrastructure`, and `Web`. The project structure is well-organized, with a clear separation of concerns. Security best practices (CSRF, Rate Limiting, Secure Headers) are largely in place. However, there are specific areas regarding **concurrency safety**, **abstraction leaks**, and **MVC best practices** that require attention.

## Layer Analysis & Ratings

### 1. Domain Layer
**Rating: 9/10**

The Domain layer is the strongest part of the solution.
- **Strengths:**
  - **Rich Domain Model:** Entities like `Event` properly encapsulate behavior (e.g., private setters, `AddGuest`, `UpdateDetails` methods) rather than being anemic property bags.
  - **Zero Dependencies:** Correctly points inward with no references to outer layers.
  - **Factories/Constructors:** Enforces invariants upon creation (e.g., date checks).
- **Minor Issues:**
  - `IEventRepository` interface contains methods that imply specific business rules (e.g., `GuestEmailExistsAsync`), which slightly blurs the line between generic data access and domain logic usage.

### 2. Application Layer
**Rating: 7/10** (Primary area for improvement)

The Application layer orchestrates logic well but implementation details lower the score.
- **Strengths:**
  - **DTO Usage:** Correctly separates Domain Entities from API/View contracts.
  - **Validation:** Uses FluentValidation effectively.
  - **Authorization:** Checks ownership (`OrganizerId`) before actions.
- **Violations & Risks:**
  - **CRITICAL: Concurrency Race Condition:** `JoinEventAsync` performs a "check-then-act" sequence (check capacity -> add guest) without a transaction lock or database constraint check. Under high load, this will lead to overbooking.
  - **Leaky Abstractions:** `IMemoryCache` is injected directly into `EventService`. Clean Architecture dictates defining an `ICacheService` interface in Application and implementing it in Infrastructure to decouple from specific caching technologies.
  - **Domain Logic Leak:** `EventService` contains private static logic `ParsePhoneNumber` which likely belongs in the Domain (e.g., a `PhoneNumber` value object factory).

### 3. Infrastructure Layer
**Rating: 8/10**

Solid implementation of data access and external concerns.
- **Strengths:**
  - **EF Core Best Practices:** Uses `AsNoTracking()` for read operations and explicit `Include()` calls.
  - **Repository Pattern:** faithfully implements Domain interfaces.
- **Issues:**
  - **Inefficient Querying:** `IsUserJoinedAsync` fetches the entire `User` entity just to check if their Email exists in the `Guest` table. This is an unnecessary roundtrip/load.
  - **Value Object Handling:** `GuestPhoneExistsAsync` instantiates a Domain Value Object inside the repository catch block to parse input. While allowed, it couples the repository closely to the specific validation rules of the Value Object constructor.

### 4. Web (MVC) Layer
**Rating: 8/10**

A secure and standard MVC implementation.
- **Strengths:**
  - **Security:** CSRF tokens (`AutoValidateAntiforgeryTokenAttribute`), Rate Limiting, and HSTS are correctly configured in `Program.cs`.
  - **Error Handling:** Global exception handler and proper status code returns.
- **Violations:**
  - **Presentation Logic in Controller:** The `MyEvents` action contains significant logic for manipulating string view states ("upcoming" vs "past") and defaulting dates. This matches the **MVC** pattern but burdens the Controller; a `ViewModelBuilder` or `Facade` would be cleaner.
  - **ViewBag Overuse:** Using `ViewBag.Venues` bypasses strong typing. A proper ViewModel (e.g., `EventFormViewModel` containing `IEnumerable<SelectListItem>`) is the preferred MVC approach.

## Vulnerabilities & Risks

1.  **Race Condition (High Risk):** As noted in Application Layer, the Event Joining flow is not thread-safe.
    *   *Remediation:* Implement optimistic concurrency (Versioning) or pessimistic locking (SQL TRANSACTION) within the repository, or enforce a unique constraint on the database side for (EventId, Email).
2.  **Hardcoded Cache Keys (Low Risk):** Keys like `"event_details_"` are hardcoded strings scattered in the service.
    *   *Remediation:* Move to a centralized `CacheKeys` constant class or builder.
3.  **UI Data Leaks (Medium Risk):** While checking `MyEvents`, ensuring `IsPrivate` events are strictly filtered based on `OrganizerId` is critical. The current `EventRepository.GetFilteredAsync` logic seems to handle this correctly (`viewerId` check), but it relies on string comparison of IDs.

## Recommendations
1.  **Fix Concurrency:** Wrap the Join logic in a `Serializable` transaction or use a database constraint to prevent overbooking.
2.  **Refactor Caching:** Extract `ICacheService` to remove `Microsoft.Extensions.Caching.Memory` dependency from the Application layer.
3.  **Strengthen ViewModels:** Replace `ViewBag` usage in `EventController` with strongly-typed ViewModels containing dropdown data.
