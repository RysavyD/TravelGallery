using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelGallery.Data;
using TravelGallery.Models;
using TravelGallery.Models.Api;
using TravelGallery.Services;

namespace TravelGallery.Controllers.Api;

[ApiController]
[Route("api/trips/{tripId}/media")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class MediaController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly FileStorageService _storage;

    public MediaController(ApplicationDbContext db, FileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(int tripId)
    {
        var trip = await _db.Trips.AsNoTracking().AnyAsync(t => t.Id == tripId);
        if (!trip) return NotFound(new { message = "Výlet nenalezen." });

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var media = await _db.Media
            .Where(m => m.TripId == tripId)
            .OrderBy(m => m.SortOrder)
            .AsNoTracking()
            .Select(m => TripsController.ToMediaDto(m, tripId, baseUrl))
            .ToListAsync();

        return Ok(media);
    }

    [HttpPost]
    [RequestSizeLimit(50_000_000)]
    public async Task<IActionResult> Upload(int tripId, [FromForm] List<IFormFile> files)
    {
        var trip = await _db.Trips.AnyAsync(t => t.Id == tripId);
        if (!trip) return NotFound(new { message = "Výlet nenalezen." });

        if (files == null || files.Count == 0)
            return BadRequest(new { message = "Žádné soubory." });

        var maxSort = await _db.Media
            .Where(m => m.TripId == tripId)
            .MaxAsync(m => (int?)m.SortOrder) ?? 0;

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var result = new List<MediaDto>();

        foreach (var file in files)
        {
            if (!_storage.IsAllowedExtension(file))
                continue;

            var (fileName, mediaType, exif) = await _storage.SaveAsync(file, tripId);

            var media = new Media
            {
                TripId = tripId,
                FileName = fileName,
                MediaType = mediaType,
                SortOrder = ++maxSort,
                CreatedAt = DateTime.UtcNow,
                DateTaken = exif?.DateTaken,
                Latitude = exif?.Latitude,
                Longitude = exif?.Longitude,
                CameraModel = exif?.CameraModel,
                ExifSummary = exif?.ExifSummary
            };

            _db.Media.Add(media);
            await _db.SaveChangesAsync();

            result.Add(TripsController.ToMediaDto(media, tripId, baseUrl));
        }

        return CreatedAtAction(nameof(GetAll), new { tripId }, result);
    }

    [HttpPut("{id}/caption")]
    public async Task<IActionResult> UpdateCaption(int tripId, int id, [FromBody] CaptionDto dto)
    {
        var media = await _db.Media.FirstOrDefaultAsync(m => m.Id == id && m.TripId == tripId);
        if (media == null) return NotFound();

        media.Caption = dto.Caption ?? "";
        await _db.SaveChangesAsync();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(TripsController.ToMediaDto(media, tripId, baseUrl));
    }

    [HttpPut("reorder")]
    public async Task<IActionResult> Reorder(int tripId, [FromBody] MediaReorderDto dto)
    {
        var mediaItems = await _db.Media
            .Where(m => m.TripId == tripId)
            .ToListAsync();

        for (int i = 0; i < dto.OrderedIds.Count; i++)
        {
            var item = mediaItems.FirstOrDefault(m => m.Id == dto.OrderedIds[i]);
            if (item != null)
                item.SortOrder = i;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int tripId, int id)
    {
        var media = await _db.Media.FirstOrDefaultAsync(m => m.Id == id && m.TripId == tripId);
        if (media == null) return NotFound();

        _storage.Delete(media.FileName, tripId);
        _db.Media.Remove(media);
        await _db.SaveChangesAsync();

        return NoContent();
    }
}

public class CaptionDto
{
    public string? Caption { get; set; }
}
