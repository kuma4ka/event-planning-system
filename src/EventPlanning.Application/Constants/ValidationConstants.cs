namespace EventPlanning.Application.Constants;

public static class ValidationConstants
{
    public const int MaxNameLength = 200;
    public const int MaxAddressLength = 200;
    public const int MaxDescriptionLength = 1000;
    public const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
    public const int MaxImageSizeInMb = 5;
    public const int MaxCapacity = 100_000;
    
    public static readonly string[] AllowedImageContentTypes = ["image/jpeg", "image/png", "image/jpg", "image/webp"];
}