# Solution Architecture Analysis Report

## Executive Summary
The solution follows a standard Clean Architecture structure with clearly separated `Domain`, `Application`, `Infrastructure`, and `Web` (Presentation) layers. While the structural foundation is solid, there are noticeable violations of separation of concerns, particularly regarding business logic adhering to the Domain, and presentation logic leaking into Controllers via `ViewBag`.

### Overall Architecture Rating: 6.5/10

---

## Layer-by-Layer Analysis

### 1. Domain Layer
**Score: 8/10**

**Strengths:**
- **Entity Guidelines:** Entities (`Event`, `Guest`, etc.) use private setters, enforcing immutability from outside the class.
- **Rich Domain Models:** Some behavior is encapsulated in the entities (e.g., `UpdateDetails`, factory methods).
- **No Dependencies:** Correctly has no references to outer layers.

**Weaknesses & Violations:**
- **Leakage of Domain Logic:** The logic for parsing and formatting phone numbers (Country Code splitting) currently resides in `EventService` (Application Layer) but conceptually belongs to the `PhoneNumber` Value Object or a Domain Service.
- **Anemic Tendencies:** While not fully anemic, complex validation (like checking capacity) is somewhat scattered between the Service and Repository, rather than being a core part of the Aggregate Root's invariants (though difficult with database-backed constraints).

### 2. Application Layer
**Score: 6/10**

**Strengths:**
- **DTO Usage:** Clear separation between Domain Entities and Data Transfer Objects (DTOs).
- **Validation:** Strong usage of `FluentValidation` for incoming requests.
- **Abstractions:** Good use of interfaces (`IIdenityService`, `ICacheService`).

**Weaknesses & Violations:**
- **Service Bloat:** The `EventService` is doing too much. It handles:
    - Object-to-DTO mapping (should be in Mappers/AutoMapper).
    - Caching logic (should be a Decorated Service or Aspect).
    - Business logic (Join rules).
    - Infrastructure orchestration.
- **Caching Logic Mixing:** `GetEventDetailsAsync` contains complex imperative caching logic (checking two different keys) mixed directly with repository calls. This violates the Single Responsibility Principle (SRP).
- **Business Logic Leakage:** As mentioned, `ParsePhoneNumber` is a private static method in the Service, which is a Domain concern.

### 3. Infrastructure Layer
**Score: 7/10**

**Strengths:**
- **EF Core Encapsulation:** Correctly implements Repository interfaces.
- **Transactional Integrity:** `TryJoinEventAsync` uses execution strategies and serializable transactions to prevent race conditions (Overbooking), which is excellent for data integrity.

**Weaknesses & Violations:**
- **Business Logic in Repository:** The `TryJoinEventAsync` method implements the "Capacity Check" business rule (`currentCount >= Capacity`) effectively inside a SQL transaction. While necessary for concurrency correctness, usually the "Rule" definition belongs in the Domain. Currently, the Repository decides what "Full" means.
- **Complex Query Construction:** `GetFilteredAsync` builds complex dynamic LINQ queries. This is acceptable but can become hard to maintain. Specification Pattern could be useful here.

### 4. Web (MVC) Layer
**Score: 5/10**

**Strengths:**
- **Attributes:** Proper use of `[Authorize]`, `[ValidateAntiForgeryToken]`.
- **Routing:** Clear REST-like routing conventions.

**Weaknesses & Violations:**
- **ViewBag Abuse:** The `EventController` relies heavily on `ViewBag` for critical UI state (`ViewBag.CurrentSort`, `ViewBag.GoogleMapsApiKey`, etc.). This creates "Magic String" dependencies and fragile views that are not type-safe.
- **Fat Controllers:** Controllers contain significant boilerplate for Exception Handling -> ModelState mapping. This should be handled by a global Exception Filter or Middleware, or a Result Pattern wrapper.
- **Presentation Logic in Controllers:** The logic to toggle sort parameters (`date_asc` <-> `date_desc`) is manually implemented in the Controller `SetMyEventsViewBag`. This is View Logic that should be in a ViewModel or TagHelper.

---

## Identified Risks & Vulnerabilities

### 1. Concurrency & Data Integrity
- **Risk:** High (Mitigated).
- **Observation:** The `TryJoinEventAsync` mitigates the specific risk of overbooking via explicit transactions. However, if any other part of the system adds guests without this specific method (e.g. an Admin bulk import), the capacity invariant could be violated because it's not enforced at the Database Constraint level or Domain Aggregate level, but in a specific Repository method.

### 2. Maintainability (Coupling)
- **Risk:** Medium.
- **Observation:** The `EventService` is highly coupled to the specific caching implementation structure. Changing how events are cached requires rewriting the core Service logic.

### 3. Type Safety (MVC)
- **Risk:** Medium.
- **Observation:** Extensive use of `ViewBag` means renaming a property or changing a requirement breaks the UI implementation silently (runtime error or invisible data) rather than getting a compile-time error.

## Recommendations
1.  **Refactor Caching:** Move Caching logic to a Decorator pattern (`CachedEventService : IEventService`) to clean up `EventService`.
2.  **Encapsulate Phone Logic:** Move `ParsePhoneNumber` logic into `EventPlanning.Domain.ValueObjects.PhoneNumber`.
3.  **Adopt ViewModels:** Replace all `ViewBag` usage with strongly-typed ViewModels (e.g., `EventListViewModel` containing `SortOrder`, `SearchTerm`, etc.).
4.  **Simplify Repositories:** Consider if the Capacity check can be enforced differently, or clearly document why it lives in the Repository (Concurrency).
