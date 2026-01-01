# Refactoring Web and Application Layers

## Goal
Address architectural violations and risks identified in the `EventPlanning` solution, specifically focusing on the Application and Web layers.

## User Review Required
> [!IMPORTANT]
> **Breaking Change**: `EventService` constructor will change to accept `ICacheService` instead of `IMemoryCache`.
> **Database Interaction**: `JoinEventAsync` will be refactored to ensure data consistency. This might involve adding new repository methods or transaction management.

## Proposed Changes

### 1. Application Layer: Fix Abstraction Leaks (Caching)
Currently, `EventService` depends directly on `Microsoft.Extensions.Caching.Memory`.
- **Create Interface**: `EventPlanning.Application.Interfaces.ICacheService`
- **Implement Interface**: `EventPlanning.Infrastructure.Services.MemoryCacheService` (wrapping `IMemoryCache`)
- **Refactor**: Update `EventService` to use `ICacheService`.

### 2. Application Layer: Fix Concurrency in `JoinEventAsync`
The "check-then-act" logic for event capacity is not thread-safe.
- **Approach**: Move the "check capacity and add" logic into a transaction or a specialized Repository method that uses database-level locking or atomic updates.
- **Change**: Introduce `IEventRepository.TryJoinEventAsync` which handles the check and insert atomically (or within a transaction scope).

### 3. Web Layer: Clean up `EventController`
- **Refactor `MyEvents`**: Move the view state determination logic (upcoming/past, date defaults) into a helper or strictly typed logic.
- **Remove `ViewBag.Venues`**:
    - Create `EventFormViewModel` that includes the `CreateEventDto`/`UpdateEventDto` and the `Venues` list.
    - Update `Create` and `Edit` views to use this ViewModel.

## Verification Plan

### Automated Tests
- Run existing tests to ensure no regression.
- `dotnet test`

### Manual Verification
- Verify `JoinEvent` flow works as expected.
- Verify `MyEvents` filtering works as expected.
- Verify `Create` and `Edit` pages load and save correctly with the new ViewModel.
