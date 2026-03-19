using System.Globalization;
using System.Security.Claims;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using TravelGallery.Data;
using TravelGallery.Models;
using TravelGallery.ViewModels;

namespace TravelGallery.Controllers;

[Authorize]
public class TripsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;

    public TripsController(ApplicationDbContext db, IWebHostEnvironment env)
    {
        _db = db;
        _env = env;
    }

    public async Task<IActionResult> Index(string? tag, string? q)
    {
        var query = _db.Trips
            .Include(t => t.Media)
            .Include(t => t.Tags)
            .OrderByDescending(t => t.Date)
            .AsQueryable();

        // Non-admin users see only trips from their groups
        if (!User.IsInRole("Admin"))
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            query = query.Where(t => t.Groups.Any(g => g.Members.Any(m => m.Id == userId)));
        }

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

    public async Task<IActionResult> ExportPdf(int id)
    {
        var trip = await _db.Trips
            .Include(t => t.Media.OrderBy(m => m.SortOrder))
            .Include(t => t.Tags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (trip == null) return NotFound();

        var description = StripHtml(trip.Description).Trim();
        var images = trip.Media.OrderBy(m => m.SortOrder).ToList();
        var wwwroot = _env.WebRootPath;

        var doc = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Content().Column(col =>
                {
                    // Název + datum
                    col.Item().Text(trip.Title).Bold().FontSize(24);
                    col.Item().PaddingTop(4).Text(
                        trip.Date.ToString("d. MMMM yyyy", new CultureInfo("cs-CZ")))
                        .FontColor(Colors.Grey.Darken1);

                    // Tagy
                    if (trip.Tags.Any())
                    {
                        col.Item().PaddingTop(8).Row(row =>
                        {
                            foreach (var tag in trip.Tags.OrderBy(t => t.Name))
                            {
                                row.AutoItem().PaddingRight(4).Border(1)
                                    .BorderColor(Colors.Grey.Lighten1)
                                    .Padding(2).Text(tag.Name).FontSize(9);
                            }
                        });
                    }

                    col.Item().PaddingTop(12).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    // Popis
                    if (!string.IsNullOrWhiteSpace(description))
                    {
                        col.Item().PaddingTop(12).Text(description).LineHeight(1.5f);
                    }

                    // Souřadnice
                    if (trip.Latitude.HasValue && trip.Longitude.HasValue)
                    {
                        col.Item().PaddingTop(8).Text(
                            $"📍 {trip.Latitude.Value.ToString("F5", CultureInfo.InvariantCulture)}, " +
                            $"{trip.Longitude.Value.ToString("F5", CultureInfo.InvariantCulture)}")
                            .FontColor(Colors.Grey.Darken1).FontSize(9);
                    }

                    // Fotografie
                    if (images.Any())
                    {
                        col.Item().PaddingTop(16).Text("Fotografie").Bold().FontSize(14);
                        col.Item().PaddingTop(8).Table(table =>
                        {
                            table.ColumnsDefinition(cols =>
                            {
                                cols.RelativeColumn();
                                cols.RelativeColumn();
                            });

                            foreach (var (m, idx) in images.Select((m, i) => (m, i)))
                            {
                                table.Cell().Row((uint)(idx / 2 * 2 + 1)).Column((uint)(idx % 2 + 1))
                                    .Padding(4).Column(imgCol =>
                                    {
                                        if (m.MediaType == MediaType.Image)
                                        {
                                            var thumbPath = Path.Combine(wwwroot, "uploads",
                                                trip.Id.ToString(), "thumbs", m.FileName);
                                            var origPath = Path.Combine(wwwroot, "uploads",
                                                trip.Id.ToString(), m.FileName);
                                            var imgPath = System.IO.File.Exists(thumbPath) ? thumbPath : origPath;

                                            if (System.IO.File.Exists(imgPath))
                                                imgCol.Item().AspectRatio(4f / 3f).Image(imgPath).FitArea();
                                        }
                                        else
                                        {
                                            imgCol.Item().AspectRatio(4f / 3f)
                                                .Background(Colors.Grey.Lighten3)
                                                .AlignCenter().AlignMiddle()
                                                .Text("▶ Video").FontColor(Colors.Grey.Darken2);
                                        }

                                        if (!string.IsNullOrEmpty(m.Caption))
                                        {
                                            imgCol.Item().PaddingTop(3)
                                                .Text(m.Caption).FontSize(8).FontColor(Colors.Grey.Darken2);
                                        }
                                    });
                            }
                        });
                    }
                });

                page.Footer().AlignRight().Text(x =>
                {
                    x.Span("Strana ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    x.CurrentPageNumber().FontSize(9).FontColor(Colors.Grey.Darken1);
                    x.Span(" / ").FontSize(9).FontColor(Colors.Grey.Darken1);
                    x.TotalPages().FontSize(9).FontColor(Colors.Grey.Darken1);
                });
            });
        });

        var bytes = doc.GeneratePdf();
        var safeTitle = string.Concat(trip.Title.Split(Path.GetInvalidFileNameChars()));
        return File(bytes, "application/pdf", $"{safeTitle}.pdf");
    }

    private static string StripHtml(string html)
    {
        if (string.IsNullOrEmpty(html)) return string.Empty;
        return Regex.Replace(html, "<[^>]+>", string.Empty);
    }
}
