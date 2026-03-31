using Ganss.Xss;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelGallery.Areas.Admin.ViewModels;
using TravelGallery.Data;
using TravelGallery.Models;
using TravelGallery.Services;

namespace TravelGallery.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class TripsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly FileStorageService _storage;
    private readonly HtmlSanitizer _sanitizer;

    public TripsController(ApplicationDbContext db, FileStorageService storage, HtmlSanitizer sanitizer)
    {
        _db = db;
        _storage = storage;
        _sanitizer = sanitizer;
    }

    public async Task<IActionResult> Index()
    {
        var trips = await _db.Trips
            .Include(t => t.Media)
            .Include(t => t.Tags)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
        return View(trips);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new TripFormViewModel { Date = DateTime.Today });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TripFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var trip = new Trip
        {
            Title = model.Title,
            Date = model.Date,
            Description = _sanitizer.Sanitize(model.Description ?? string.Empty),
            Latitude = model.Latitude,
            Longitude = model.Longitude
        };
        _db.Trips.Add(trip);
        await _db.SaveChangesAsync();

        await SaveTagsAsync(trip, model.TagNames);

        return RedirectToAction("Upload", "Media", new { tripId = trip.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var trip = await _db.Trips.Include(t => t.Tags).FirstOrDefaultAsync(t => t.Id == id);
        if (trip == null) return NotFound();

        return View(new TripFormViewModel
        {
            Id = trip.Id,
            Title = trip.Title,
            Date = trip.Date,
            Description = trip.Description,
            TagNames = string.Join(", ", trip.Tags.Select(t => t.Name)),
            Latitude = trip.Latitude,
            Longitude = trip.Longitude
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TripFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var trip = await _db.Trips.Include(t => t.Tags).FirstOrDefaultAsync(t => t.Id == model.Id);
        if (trip == null) return NotFound();

        trip.Title = model.Title;
        trip.Date = model.Date;
        trip.Description = _sanitizer.Sanitize(model.Description ?? string.Empty);
        trip.Latitude = model.Latitude;
        trip.Longitude = model.Longitude;

        await SaveTagsAsync(trip, model.TagNames);

        TempData["Success"] = "Výlet byl uložen.";
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var trip = await _db.Trips.Include(t => t.Media).FirstOrDefaultAsync(t => t.Id == id);
        if (trip == null) return NotFound();
        return View(trip);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var trip = await _db.Trips.Include(t => t.Media).FirstOrDefaultAsync(t => t.Id == id);
        if (trip == null) return NotFound();

        _storage.DeleteTripFolder(id);
        _db.Trips.Remove(trip);
        await _db.SaveChangesAsync();

        TempData["Success"] = "Výlet byl smazán.";
        return RedirectToAction("Index");
    }

    // ── helpers ──────────────────────────────────────────────────────────────

    private async Task SaveTagsAsync(Trip trip, string? tagNames)
    {
        // Parse comma-separated tag names
        var names = (tagNames ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(n => n.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        // Clear existing tags on trip
        trip.Tags.Clear();

        foreach (var name in names)
        {
            var slug = Tag.ToSlug(name);
            var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Slug == slug)
                      ?? new Tag { Name = name, Slug = slug };

            if (tag.Id == 0) _db.Tags.Add(tag);
            trip.Tags.Add(tag);
        }

        await _db.SaveChangesAsync();
    }

}
