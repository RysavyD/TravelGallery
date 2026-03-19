using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TravelGallery.Areas.Admin.ViewModels;
using TravelGallery.Data;
using TravelGallery.Models;

namespace TravelGallery.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class GroupsController : Controller
{
    private readonly ApplicationDbContext _db;

    public GroupsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var groups = await _db.TravelGroups
            .Include(g => g.Trips)
            .Include(g => g.Members)
            .OrderBy(g => g.Name)
            .ToListAsync();

        return View(groups);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int? id)
    {
        var vm = new GroupFormViewModel
        {
            AllTrips = await _db.Trips
                .OrderByDescending(t => t.Date)
                .Select(t => new SelectListItem(
                    $"{t.Title} ({t.Date:d. M. yyyy})", t.Id.ToString()))
                .ToListAsync(),
            AllUsers = await _db.Users
                .OrderBy(u => u.DisplayName)
                .Select(u => new SelectListItem(
                    $"{u.DisplayName} ({u.Email})", u.Id))
                .ToListAsync()
        };

        if (id.HasValue)
        {
            var group = await _db.TravelGroups
                .Include(g => g.Trips)
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == id.Value);

            if (group == null) return NotFound();

            vm.Id = group.Id;
            vm.Name = group.Name;
            vm.SelectedTripIds = group.Trips.Select(t => t.Id).ToList();
            vm.SelectedUserIds = group.Members.Select(m => m.Id).ToList();
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(GroupFormViewModel vm)
    {
        if (!ModelState.IsValid)
        {
            vm.AllTrips = await _db.Trips
                .OrderByDescending(t => t.Date)
                .Select(t => new SelectListItem(
                    $"{t.Title} ({t.Date:d. M. yyyy})", t.Id.ToString()))
                .ToListAsync();
            vm.AllUsers = await _db.Users
                .OrderBy(u => u.DisplayName)
                .Select(u => new SelectListItem(
                    $"{u.DisplayName} ({u.Email})", u.Id))
                .ToListAsync();
            return View(vm);
        }

        TravelGroup group;

        if (vm.Id == 0)
        {
            group = new TravelGroup { Name = vm.Name };
            _db.TravelGroups.Add(group);
            await _db.SaveChangesAsync();
        }
        else
        {
            group = await _db.TravelGroups
                .Include(g => g.Trips)
                .Include(g => g.Members)
                .FirstOrDefaultAsync(g => g.Id == vm.Id)
                ?? throw new InvalidOperationException("Skupina nenalezena.");

            group.Name = vm.Name;
        }

        // Synchronizace výletů
        var selectedTrips = await _db.Trips
            .Where(t => vm.SelectedTripIds.Contains(t.Id))
            .ToListAsync();
        group.Trips.Clear();
        foreach (var trip in selectedTrips)
            group.Trips.Add(trip);

        // Synchronizace členů
        var selectedUsers = await _db.Users
            .Where(u => vm.SelectedUserIds.Contains(u.Id))
            .ToListAsync();
        group.Members.Clear();
        foreach (var user in selectedUsers)
            group.Members.Add(user);

        await _db.SaveChangesAsync();

        TempData["Success"] = vm.Id == 0
            ? $"Skupina \"{group.Name}\" byla vytvořena."
            : $"Skupina \"{group.Name}\" byla uložena.";

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var group = await _db.TravelGroups.FindAsync(id);
        if (group != null)
        {
            _db.TravelGroups.Remove(group);
            await _db.SaveChangesAsync();
            TempData["Success"] = $"Skupina \"{group.Name}\" byla smazána.";
        }
        return RedirectToAction(nameof(Index));
    }
}
