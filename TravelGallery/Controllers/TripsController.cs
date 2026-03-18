using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelGallery.Data;
using TravelGallery.Models;
using TravelGallery.ViewModels;

namespace TravelGallery.Controllers;

[Authorize]
public class TripsController : Controller
{
    private readonly ApplicationDbContext _db;

    public TripsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? tag, string? q)
    {
        var query = _db.Trips
            .Include(t => t.Media)
            .Include(t => t.Tags)
            .OrderByDescending(t => t.Date)
            .AsQueryable();

        if (!string.IsNullOrEmpty(tag))
            query = query.Where(t => t.Tags.Any(tg => tg.Slug == tag));

        if (!string.IsNullOrEmpty(q))
        {
            var search = q.Trim();
            query = query.Where(t =>
                t.Title.Contains(search) ||
                t.Description.Contains(search) ||
                t.Tags.Any(tg => tg.Name.Contains(search)));
        }

        var trips = await query.ToListAsync();

        var viewModels = trips.Select(t =>
        {
            var plainText = StripHtml(t.Description);
            return new TripTimelineItemViewModel
            {
                Id = t.Id,
                Title = t.Title,
                Date = t.Date,
                ShortDescription = plainText.Length > 300
                    ? plainText[..300] + "…"
                    : plainText,
                ThumbnailUrls = t.Media
                    .OrderBy(m => m.SortOrder)
                    .Where(m => m.MediaType == MediaType.Image)
                    .Take(4)
                    .Select(m => $"/uploads/{t.Id}/thumbs/{m.FileName}")
                    .ToList(),
                MediaCount = t.Media.Count,
                Tags = t.Tags.OrderBy(tg => tg.Name)
                    .Select(tg => (tg.Name, tg.Slug))
                    .ToList(),
                Latitude = t.Latitude,
                Longitude = t.Longitude
            };
        }).ToList();

        ViewBag.ActiveTag = tag;
        ViewBag.SearchQuery = q;
        return View(viewModels);
    }

    public async Task<IActionResult> Detail(int id)
    {
        var trip = await _db.Trips
            .Include(t => t.Media.OrderBy(m => m.SortOrder))
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null) return NotFound();

        return View(trip);
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        return Regex.Replace(html, "<[^>]+>", string.Empty);
    }
}
