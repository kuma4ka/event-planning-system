using EventPlanning.Application.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace EventPlanning.Infrastructure.Services;

public class ImageService(IWebHostEnvironment webHostEnvironment) : IImageService
{
    public async Task<string> UploadImageAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default)
    {
        var webRootPath = webHostEnvironment.WebRootPath;
        
        var uploadPath = Path.Combine(webRootPath, "images", folderName);
        if (!Directory.Exists(uploadPath))
        {
            Directory.CreateDirectory(uploadPath);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
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