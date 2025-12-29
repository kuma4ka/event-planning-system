using EventPlanning.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EventPlanning.Infrastructure.Services;

public class ImageService(IWebHostEnvironment webHostEnvironment) : IImageService
{
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public async Task<string> UploadImageAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default)
    {
        var webRootPath = webHostEnvironment.WebRootPath;
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (string.IsNullOrEmpty(extension) || !_allowedExtensions.Contains(extension))
        {
            throw new ArgumentException($"Invalid file type. Allowed types: {string.Join(", ", _allowedExtensions)}");
        }

        if (!file.ContentType.StartsWith("image/"))
        {
            throw new ArgumentException("Invalid file content type. Only images are allowed.");
        }

        var uploadPath = Path.Combine(webRootPath, "images", folderName);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + extension;
        var filePath = Path.Combine(uploadPath, uniqueFileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        return $"/images/{folderName}/{uniqueFileName}";
    }

    public void DeleteImage(string imageUrl)
    {
        if (string.IsNullOrEmpty(imageUrl)) return;

        var webRootPath = webHostEnvironment.WebRootPath;
        var filePath = Path.Combine(webRootPath, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
    }
}