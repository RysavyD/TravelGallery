using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelGallery.Data;
using TravelGallery.Models;
using TravelGallery.Services;

namespace TravelGallery.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class MediaController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly FileStorageService _storage;

    public MediaController(ApplicationDbContext db, FileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    [HttpGet]
    public async Task<IActionResult> Upload(int tripId)
    {
        var trip = await _db.Trips
            .Include(t => t.Media.OrderBy(m => m.SortOrder))
            .FirstOrDefaultAsync(t => t.Id == tripId);

        if (trip == null) return NotFound();

        return View(trip);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int tripId, List<IFormFile> files)
    {
        var trip = await _db.Trips.FindAsync(tripId);
        if (trip == null) return NotFound();

        var maxOrder = await _db.Media
            .Where(m => m.TripId == tripId)
            .Select(m => (int?)m.SortOrder)
            .MaxAsync() ?? 0;

        int savedCount = 0;
        foreach (var file in files)
        {
            if (file.Length == 0 || !_storage.IsAllowedExtension(file))
                continue;

            var (fileName, mediaType) = await _storage.SaveAsync(file, tripId);

            _db.Media.Add(new Media
            {
                TripId = tripId,
                FileName = fileName,
                MediaType = mediaType,
                Caption = Path.GetFileNameWithoutExtension(file.FileName),
                SortOrder = ++maxOrder
            });
            savedCount++;
        }

        await _db.SaveChangesAsync();

        if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            return Json(new { count = savedCount });

        TempData["Success"] = $"Nahráno {savedCount} souborů.";
        return RedirectToAction("Upload", new { tripId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCaption(int id, string caption)
    {
        var media = await _db.Media.FindAsync(id);
        if (media == null) return NotFound();

        media.Caption = caption ?? string.Empty;
        await _db.SaveChangesAsync();

        return RedirectToAction("Upload", new { tripId = media.TripId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var media = await _db.Media.FindAsync(id);
        if (media == null) return NotFound();

        var tripId = media.TripId;
        _storage.Delete(media.FileName, tripId);
        _db.Media.Remove(media);
        await _db.SaveChangesAsync();

        return RedirectToAction("Upload", new { tripId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reorder(int tripId, [FromBody] List<int> orderedIds)
    {
        var media = await _db.Media.Where(m => m.TripId == tripId).ToListAsync();

        for (int i = 0; i < orderedIds.Count; i++)
        {
            var item = media.FirstOrDefault(m => m.Id == orderedIds[i]);
            if (item != null) item.SortOrder = i + 1;
        }

        await _db.SaveChangesAsync();
        return Ok();
    }
}
