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

    public TripsController(ApplicationDbContext db, FileStorageService storage)
    {
        _db = db;
        _storage = storage;
    }

    public async Task<IActionResult> Index()
    {
        var trips = await _db.Trips
            .Include(t => t.Media)
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
            Description = model.Description
        };
        _db.Trips.Add(trip);
        await _db.SaveChangesAsync();

        return RedirectToAction("Upload", "Media", new { tripId = trip.Id });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var trip = await _db.Trips.FindAsync(id);
        if (trip == null) return NotFound();

        return View(new TripFormViewModel
        {
            Id = trip.Id,
            Title = trip.Title,
            Date = trip.Date,
            Description = trip.Description
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TripFormViewModel model)
    {
        if (!ModelState.IsValid) return View(model);

        var trip = await _db.Trips.FindAsync(model.Id);
        if (trip == null) return NotFound();

        trip.Title = model.Title;
        trip.Date = model.Date;
        trip.Description = model.Description;
        await _db.SaveChangesAsync();

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
}
