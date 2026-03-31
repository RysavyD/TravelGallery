using TravelGallery.Data;
using TravelGallery.Models;

namespace TravelGallery.Tests.Fixtures;

/// <summary>
/// Seeduje testovací data do in-memory DB.
/// </summary>
public static class TestDbSeeder
{
    public static void Seed(ApplicationDbContext db)
    {
        if (db.Trips.Any()) return; // Zabrání dvojitému seeding

        var tagHory = new Tag { Name = "Hory",   Slug = "hory" };
        var tagZima = new Tag { Name = "Zima",   Slug = "zima" };
        var tagKolo = new Tag { Name = "Cyklo",  Slug = "cyklo" };

        db.Trips.AddRange(
            new Trip
            {
                Title       = "Výlet na Lysou horu",
                Date        = new DateTime(2024, 7, 1),
                Description = "<p>Krásný výlet na Lysou horu.</p>",
                Tags        = new List<Tag> { tagHory }
            },
            new Trip
            {
                Title       = "Zimní turistika Špindl",
                Date        = new DateTime(2024, 1, 15),
                Description = "<p>Zima ve Špindlerovem Mlýně.</p>",
                Tags        = new List<Tag> { tagHory, tagZima }
            },
            new Trip
            {
                Title       = "Cyklovýlet Jeseníky",
                Date        = new DateTime(2024, 8, 20),
                Description = "<p>Jeseníky na kole – skvělá trasa.</p>",
                Tags        = new List<Tag> { tagKolo }
            },
            new Trip
            {
                Title       = "Výlet bez tagů",
                Date        = new DateTime(2023, 5, 10),
                Description = "<p>Krátký výlet.</p>",
                Tags        = new List<Tag>()
            }
        );

        db.SaveChanges();
    }
}
