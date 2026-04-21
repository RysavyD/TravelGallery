using System.IO.Compression;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TravelGallery.Data;
using TravelGallery.Models;

namespace TravelGallery.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class BackupController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly UserManager<ApplicationUser> _userManager;

    public BackupController(ApplicationDbContext db, IWebHostEnvironment env, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _env = env;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Download()
    {
        var trips = await _db.Trips
            .Include(t => t.Media)
            .Include(t => t.Tags)
            .Include(t => t.Groups)
            .AsNoTracking()
            .ToListAsync();

        var tags = await _db.Tags.AsNoTracking().ToListAsync();

        var groups = await _db.TravelGroups
            .Include(g => g.Members)
            .AsNoTracking()
            .ToListAsync();

        var users = await _db.Users.AsNoTracking().ToListAsync();
        var userRoles = new Dictionary<string, List<string>>();
        foreach (var u in users)
        {
            userRoles[u.Id] = (await _userManager.GetRolesAsync(u)).ToList();
        }

        var backup = new
        {
            exportedAt = DateTime.UtcNow,
            version = 1,
            trips = trips.Select(t => new
            {
                t.Id, t.Title, t.Date, t.Description, t.CreatedAt,
                t.Latitude, t.Longitude, t.ViewCount,
                tagIds = t.Tags.Select(x => x.Id).ToList(),
                groupIds = t.Groups.Select(g => g.Id).ToList(),
                media = t.Media.OrderBy(m => m.SortOrder).Select(m => new
                {
                    m.Id, m.FileName, m.MediaType, m.Caption, m.SortOrder, m.CreatedAt,
                    m.DateTaken, m.Latitude, m.Longitude, m.CameraModel, m.ExifSummary
                })
            }),
            tags = tags.Select(t => new { t.Id, t.Name, t.Slug }),
            groups = groups.Select(g => new
            {
                g.Id, g.Name,
                memberIds = g.Members.Select(m => m.Id).ToList()
            }),
            users = users.Select(u => new
            {
                u.Id, u.Email, u.UserName, u.DisplayName,
                roles = userRoles.TryGetValue(u.Id, out var r) ? r : new List<string>()
            })
        };

        var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(backup, new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        });

        var timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm");
        var filename = $"TravelGallery-backup-{timestamp}.zip";

        // Zapsat do dočasného souboru – spolehlivější než přímý zápis do Response.Body
        var tempPath = Path.GetTempFileName();
        try
        {
            await using (var zipStream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create))
            {
                // backup.json
                var jsonEntry = archive.CreateEntry("backup.json", CompressionLevel.Optimal);
                await using (var entryStream = jsonEntry.Open())
                {
                    await entryStream.WriteAsync(jsonBytes);
                }

                // README.txt
                var readme = $"TravelGallery záloha\n" +
                             $"=====================\n\n" +
                             $"Vytvořeno: {DateTime.Now:d.M.yyyy HH:mm}\n" +
                             $"Výletů: {trips.Count}\n" +
                             $"Fotek a videí: {trips.Sum(t => t.Media.Count)}\n" +
                             $"Uživatelů: {users.Count}\n\n" +
                             $"Obsah:\n" +
                             $"- backup.json – všechna data (bez hesel uživatelů)\n" +
                             $"- uploads/ – originály fotek/videí + thumbnaily\n";
                var readmeEntry = archive.CreateEntry("README.txt", CompressionLevel.Optimal);
                await using (var entryStream = readmeEntry.Open())
                {
                    await entryStream.WriteAsync(Encoding.UTF8.GetBytes(readme));
                }

                // uploads/
                var uploadsDir = Path.Combine(_env.WebRootPath, "uploads");
                if (Directory.Exists(uploadsDir))
                {
                    foreach (var file in Directory.EnumerateFiles(uploadsDir, "*", SearchOption.AllDirectories))
                    {
                        var relative = Path.GetRelativePath(_env.WebRootPath, file).Replace('\\', '/');
                        var entry = archive.CreateEntry(relative, CompressionLevel.NoCompression); // fotky už jsou komprimované
                        await using var entryStream = entry.Open();
                        await using var fileStream = System.IO.File.OpenRead(file);
                        await fileStream.CopyToAsync(entryStream);
                    }
                }
            }

            // Vrátit soubor a smazat ho po odeslání
            var readStream = new FileStream(tempPath, FileMode.Open, FileAccess.Read, FileShare.None,
                bufferSize: 4096, FileOptions.DeleteOnClose | FileOptions.Asynchronous);
            return File(readStream, "application/zip", filename);
        }
        catch
        {
            if (System.IO.File.Exists(tempPath)) System.IO.File.Delete(tempPath);
            throw;
        }
    }
}
