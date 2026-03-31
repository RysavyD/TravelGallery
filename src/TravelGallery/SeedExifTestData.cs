using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using TravelGallery.Data;
using TravelGallery.Models;

namespace TravelGallery;

public static class SeedExifTestData
{
    private static readonly (string City, double Lat, double Lng)[] Locations = new[]
    {
        ("Praha", 50.0755, 14.4378),
        ("Brno", 49.1951, 16.6068),
        ("Český Krumlov", 48.8127, 14.3175),
        ("Karlovy Vary", 50.2318, 12.8714),
        ("Olomouc", 49.5938, 17.2509),
        ("Kutná Hora", 49.9481, 15.2681),
        ("Telč", 49.1844, 15.4528),
        ("Plzeň", 49.7384, 13.3736),
        ("Liberec", 50.7671, 15.0562),
        ("Znojmo", 48.8555, 16.0488),
        ("Třebíč", 49.2148, 15.8816),
        ("Mikulov", 48.8056, 16.6378),
        ("Litomyšl", 49.8683, 16.3131),
        ("Kroměříž", 49.2976, 17.3931),
        ("Lednice", 48.7998, 16.8038),
    };

    private static readonly string[] Cameras = new[]
    {
        "iPhone 15 Pro", "Canon EOS R5", "Sony A7 IV", "Nikon Z6 III",
        "Samsung Galaxy S24", "Google Pixel 8 Pro", "Fujifilm X-T5"
    };

    public static async Task SeedAsync(ApplicationDbContext db, IWebHostEnvironment env)
    {
        var lastTrip = await db.Trips
            .OrderByDescending(t => t.Date)
            .FirstOrDefaultAsync();

        if (lastTrip == null) return;

        var tripId = lastTrip.Id;
        var uploadDir = Path.Combine(env.WebRootPath, "uploads", tripId.ToString());
        var thumbDir = Path.Combine(uploadDir, "thumbs");
        Directory.CreateDirectory(thumbDir);

        var maxOrder = await db.Media
            .Where(m => m.TripId == tripId)
            .Select(m => (int?)m.SortOrder)
            .MaxAsync() ?? 0;

        var rng = new Random(42);
        var baseDate = new DateTime(2025, 7, 15, 8, 0, 0);

        for (int i = 0; i < 15; i++)
        {
            var loc = Locations[i];
            var camera = Cameras[rng.Next(Cameras.Length)];
            var dateTaken = baseDate.AddHours(i * 2).AddMinutes(rng.Next(60));
            var fNumber = new[] { "f/1.8", "f/2.0", "f/2.8", "f/4.0", "f/5.6" }[rng.Next(5)];
            var exposure = new[] { "1/60", "1/125", "1/250", "1/500", "1/1000" }[rng.Next(5)];
            var iso = new[] { 100, 200, 400, 800 }[rng.Next(4)];

            // Create a colorful test image
            var hue = (int)(i * 24.0); // 0-360 spread
            var color = ColorFromHSL(hue, 0.7, 0.5 + rng.NextDouble() * 0.2);

            var fileName = $"{Guid.NewGuid()}.jpg";
            var filePath = Path.Combine(uploadDir, fileName);
            var thumbPath = Path.Combine(thumbDir, fileName);

            using (var image = new Image<Rgba32>(1920, 1080, color))
            {
                // Draw a simple pattern
                image.Mutate(ctx => ctx
                    .GaussianBlur(3)
                );

                // Write EXIF
                image.Metadata.ExifProfile = new ExifProfile();
                image.Metadata.ExifProfile.SetValue(ExifTag.DateTimeOriginal, dateTaken.ToString("yyyy:MM:dd HH:mm:ss"));
                image.Metadata.ExifProfile.SetValue(ExifTag.Model, camera);
                image.Metadata.ExifProfile.SetValue(ExifTag.Make, camera.Split(' ')[0]);

                // GPS
                image.Metadata.ExifProfile.SetValue(ExifTag.GPSLatitudeRef, loc.Lat >= 0 ? "N" : "S");
                image.Metadata.ExifProfile.SetValue(ExifTag.GPSLatitude, ToExifGpsRational(Math.Abs(loc.Lat)));
                image.Metadata.ExifProfile.SetValue(ExifTag.GPSLongitudeRef, loc.Lng >= 0 ? "E" : "W");
                image.Metadata.ExifProfile.SetValue(ExifTag.GPSLongitude, ToExifGpsRational(Math.Abs(loc.Lng)));

                await image.SaveAsJpegAsync(filePath);

                // Thumbnail
                image.Mutate(x => x.Resize(new ResizeOptions { Mode = ResizeMode.Max, Size = new Size(400, 300) }));
                await image.SaveAsJpegAsync(thumbPath);
            }

            db.Media.Add(new Media
            {
                TripId = tripId,
                FileName = fileName,
                MediaType = MediaType.Image,
                Caption = $"📸 {loc.City}",
                SortOrder = ++maxOrder,
                DateTaken = dateTaken,
                Latitude = loc.Lat,
                Longitude = loc.Lng,
                CameraModel = camera,
                ExifSummary = $"{fNumber} · {exposure}s · ISO {iso}"
            });
        }

        await db.SaveChangesAsync();
    }

    private static Rational[] ToExifGpsRational(double decimalDegrees)
    {
        var degrees = (int)decimalDegrees;
        var minutesDecimal = (decimalDegrees - degrees) * 60;
        var minutes = (int)minutesDecimal;
        var seconds = (minutesDecimal - minutes) * 60;

        return new[]
        {
            new Rational((uint)degrees, 1),
            new Rational((uint)minutes, 1),
            new Rational((uint)(seconds * 100), 100)
        };
    }

    private static Rgba32 ColorFromHSL(int hue, double saturation, double lightness)
    {
        double c = (1 - Math.Abs(2 * lightness - 1)) * saturation;
        double x = c * (1 - Math.Abs((hue / 60.0) % 2 - 1));
        double m = lightness - c / 2;

        double r, g, b;
        if (hue < 60) { r = c; g = x; b = 0; }
        else if (hue < 120) { r = x; g = c; b = 0; }
        else if (hue < 180) { r = 0; g = c; b = x; }
        else if (hue < 240) { r = 0; g = x; b = c; }
        else if (hue < 300) { r = x; g = 0; b = c; }
        else { r = c; g = 0; b = x; }

        return new Rgba32(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255));
    }
}
