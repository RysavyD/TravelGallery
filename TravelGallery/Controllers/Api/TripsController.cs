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
[Route("api/trips")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
public class TripsController : ControllerBase
{
    private readonly ApplicationDbContext _db;
    private readonly FileStorageService _storage;

    public TripsController(ApplicationDbContext db, FileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var query = _db.Trips
            .Include(t => t.Media)
            .OrderByDescending(t => t.Date)
            .AsNoTracking();

        var total = await query.CountAsync();
        var trips = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => ToDto(t))
            .ToListAsync();

        return Ok(new { total, page, pageSize, data = trips });
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var trip = await _db.Trips
            .Include(t => t.Media.OrderBy(m => m.SortOrder))
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null) return NotFound();

        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        return Ok(new TripDetailDto
        {
            Trip = ToDto(trip),
            Media = trip.Media.Select(m => ToMediaDto(m, trip.Id, baseUrl)).ToList()
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TripCreateDto dto)
    {
        var trip = new Trip
        {
            Title = dto.Title,
            Date = dto.Date,
            Description = dto.Description ?? "",
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            CreatedAt = DateTime.UtcNow
        };

        _db.Trips.Add(trip);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = trip.Id }, ToDto(trip));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] TripCreateDto dto)
    {
        var trip = await _db.Trips.FindAsync(id);
        if (trip == null) return NotFound();

        trip.Title = dto.Title;
        trip.Date = dto.Date;
        trip.Description = dto.Description ?? "";
        trip.Latitude = dto.Latitude;
        trip.Longitude = dto.Longitude;

        await _db.SaveChangesAsync();
        return Ok(ToDto(trip));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var trip = await _db.Trips.FindAsync(id);
        if (trip == null) return NotFound();

        _storage.DeleteTripFolder(id);
        _db.Trips.Remove(trip);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static TripDto ToDto(Trip t) => new()
    {
        Id = t.Id,
        Title = t.Title,
        Date = t.Date,
        Description = t.Description ?? "",
        Latitude = t.Latitude,
        Longitude = t.Longitude,
        PhotoCount = t.Media?.Count ?? 0,
        CoverPhotoUrl = t.Media?.OrderBy(m => m.SortOrder).FirstOrDefault()?.FileName
    };

    internal static MediaDto ToMediaDto(Media m, int tripId, string baseUrl) => new()
    {
        Id = m.Id,
        FileName = m.FileName,
        MediaType = m.MediaType.ToString(),
        Caption = m.Caption ?? "",
        SortOrder = m.SortOrder,
        Url = $"{baseUrl}/uploads/{tripId}/{m.FileName}",
        ThumbnailUrl = $"{baseUrl}/uploads/{tripId}/thumbs/{m.FileName}",
        DateTaken = m.DateTaken,
        CameraModel = m.CameraModel,
        ExifSummary = m.ExifSummary
    };
}
