using System.Globalization;
using MetadataExtractor;
using Directory = System.IO.Directory;
using MetadataExtractor.Formats.Exif;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using TravelGallery.Models;

namespace TravelGallery.Services;

public record ExifData(
    DateTime? DateTaken,
    double? Latitude,
    double? Longitude,
    string? CameraModel,
    string? ExifSummary);

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

    public async Task<(string FileName, MediaType MediaType, ExifData? Exif)> SaveAsync(IFormFile file, int tripId)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        var mediaType = ImageExtensions.Contains(ext) ? MediaType.Image : MediaType.Video;

        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", tripId.ToString());
        System.IO.Directory.CreateDirectory(uploadDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadDir, fileName);

        await using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        ExifData? exif = null;
        if (mediaType == MediaType.Image)
        {
            await CreateThumbnailAsync(filePath, uploadDir, fileName);
            exif = ExtractExifData(filePath);
        }

        return (fileName, mediaType, exif);
    }

    public static ExifData? ExtractExifData(string filePath)
    {
        try
        {
            var directories = ImageMetadataReader.ReadMetadata(filePath);

            // Date taken
            DateTime? dateTaken = null;
            var exifSub = directories.OfType<ExifSubIfdDirectory>().FirstOrDefault();
            if (exifSub != null && exifSub.TryGetDateTime(ExifDirectoryBase.TagDateTimeOriginal, out var dt))
                dateTaken = dt;

            // GPS
            double? lat = null, lng = null;
            var gps = directories.OfType<GpsDirectory>().FirstOrDefault();
            if (gps != null)
            {
                var location = gps.GetGeoLocation();
                if (location != null)
                {
                    lat = location.Value.Latitude;
                    lng = location.Value.Longitude;
                }
            }

            // Camera model
            string? cameraModel = null;
            var exifIfd0 = directories.OfType<ExifIfd0Directory>().FirstOrDefault();
            if (exifIfd0 != null)
            {
                var make = exifIfd0.GetDescription(ExifDirectoryBase.TagMake)?.Trim();
                var model = exifIfd0.GetDescription(ExifDirectoryBase.TagModel)?.Trim();
                if (!string.IsNullOrEmpty(model))
                {
                    // Avoid "Apple Apple iPhone 15 Pro" – if model starts with make, skip make
                    cameraModel = !string.IsNullOrEmpty(make) && !model.StartsWith(make, StringComparison.OrdinalIgnoreCase)
                        ? $"{make} {model}"
                        : model;
                }
            }

            // Exposure summary: f/1.8 · 1/120s · ISO 200
            var parts = new List<string>();
            if (exifSub != null)
            {
                var fNumber = exifSub.GetDescription(ExifDirectoryBase.TagFNumber);
                if (!string.IsNullOrEmpty(fNumber)) parts.Add(fNumber);

                var exposure = exifSub.GetDescription(ExifDirectoryBase.TagExposureTime);
                if (!string.IsNullOrEmpty(exposure)) parts.Add(exposure);

                var iso = exifSub.GetDescription(ExifDirectoryBase.TagIsoEquivalent);
                if (!string.IsNullOrEmpty(iso)) parts.Add($"ISO {iso}");
            }
            var exifSummary = parts.Count > 0 ? string.Join(" · ", parts) : null;

            // Return null if nothing was found
            if (dateTaken == null && lat == null && cameraModel == null && exifSummary == null)
                return null;

            return new ExifData(dateTaken, lat, lng, cameraModel, exifSummary);
        }
        catch
        {
            return null;
        }
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
