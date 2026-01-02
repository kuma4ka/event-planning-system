namespace EventPlanning.Application.Constants;

public static class CacheKeyGenerator
{
    private const string EventPrefix = "event_details_";

    public static string GetEventKeyPublic(Guid eventId) => $"{EventPrefix}{eventId}_public";
    public static string GetEventKeyOrganizer(Guid eventId) => $"{EventPrefix}{eventId}_organizer";
}
