using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TravelGallery.Models;

namespace TravelGallery.Services;

public class FileStorageService
{
    private readonly IWebHostEnvironment _env;

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".mov", ".avi", ".webm" };

    public FileStorageService(IWebHostEnvironment env)
    {
        _env = env;
    }

    public async Task<(string FileName, MediaType MediaType)> SaveAsync(IFormFile file, int tripId)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var mediaType = ImageExtensions.Contains(ext) ? MediaType.Image : MediaType.Video;

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", tripId.ToString());
        Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        if (mediaType == MediaType.Image)
        {
            await CreateThumbnailAsync(filePath, uploadDir, fileName);
        }

        return (fileName, mediaType);
    }

    public void Delete(string fileName, int tripId)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", tripId.ToString());

        var filePath = Path.Combine(uploadDir, fileName);
        if (File.Exists(filePath)) File.Delete(filePath);

        var thumbPath = Path.Combine(uploadDir, "thumbs", fileName);
        if (File.Exists(thumbPath)) File.Delete(thumbPath);
    }

    public void DeleteTripFolder(int tripId)
    {
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", tripId.ToString());
        if (Directory.Exists(uploadDir))
            Directory.Delete(uploadDir, recursive: true);
    }

    public bool IsAllowedExtension(IFormFile file)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        return ImageExtensions.Contains(ext) || VideoExtensions.Contains(ext);
    }

    private static async Task CreateThumbnailAsync(string sourcePath, string uploadDir, string fileName)
    {
        var thumbDir = Path.Combine(uploadDir, "thumbs");
        Directory.CreateDirectory(thumbDir);
        var thumbPath = Path.Combine(thumbDir, fileName);

        using var image = await Image.LoadAsync(sourcePath);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Mode = ResizeMode.Max,
            Size = new Size(400, 300)
        }));
        await image.SaveAsync(thumbPath);
    }
}
