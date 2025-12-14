using Microsoft.AspNetCore.Http;

namespace EventPlanning.Application.Interfaces;

public interface IImageService
{
    Task<string> UploadImageAsync(IFormFile file, string folderName, CancellationToken cancellationToken = default);
    
    void DeleteImage(string imageUrl);
}