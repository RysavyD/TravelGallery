using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TravelGallery.Data;
using TravelGallery.Models;
using TravelGallery.Tests.Fixtures;

namespace TravelGallery.Tests.Integration;

/// <summary>
/// Ověřuje logiku správy tagů – vytváření, sdílení a case-insensitivní deduplikaci.
/// </summary>
public class TagManagementTests : IClassFixture<AuthenticatedWebApplicationFactory>
{
    private readonly AuthenticatedWebApplicationFactory _factory;

    public TagManagementTests(AuthenticatedWebApplicationFactory factory)
    {
        _factory = factory;
    }

    /// <summary>
    /// Simuluje logiku SaveTagsAsync z Admin TripsController.
    /// </summary>
    private static async Task AttachTagsToTrip(ApplicationDbContext db, Trip trip, string tagNamesRaw)
    {
        var names = (tagNamesRaw ?? "")
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(n => n.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        trip.Tags.Clear();

        foreach (var name in names)
        {
            var slug = Tag.ToSlug(name);
            var tag  = await db.Tags.FirstOrDefaultAsync(t => t.Slug == slug)
                       ?? new Tag { Name = name, Slug = slug };
            if (tag.Id == 0) db.Tags.Add(tag);
            trip.Tags.Add(tag);
        }

        await db.SaveChangesAsync();
    }

    // ── nové tagy jsou vytvořeny a přiřazeny ─────────────────────────────────

    [Fact]
    public async Task SaveTags_NewTags_AreCreatedAndLinkedToTrip()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var trip = new Trip { Title = "Tag test trip", Date = DateTime.Today, Description = "" };
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        await AttachTagsToTrip(db, trip, "Hory2, Léto2, Slovensko2");

        var saved = await db.Trips.Include(t => t.Tags).FirstAsync(t => t.Id == trip.Id);
        saved.Tags.Should().HaveCount(3,
            because: "3 tagy musí být přiřazeny");
        saved.Tags.Select(t => t.Name)
            .Should().BeEquivalentTo(new[] { "Hory2", "Léto2", "Slovensko2" });
    }

    // ── existující tag je znovupoužit, ne zduplikován ────────────────────────

    [Fact]
    public async Task SaveTags_ExistingTag_IsReusedNotDuplicated()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Předem vytvoř tag
        const string tagSlug = "jeseniky-test";
        var existing = new Tag { Name = "Jeseníky-test", Slug = tagSlug };
        db.Tags.Add(existing);
        await db.SaveChangesAsync();

        // Dva různé tripy se stejným tagem
        var trip1 = new Trip { Title = "Trip s Jeseníky 1", Date = DateTime.Today, Description = "" };
        var trip2 = new Trip { Title = "Trip s Jeseníky 2", Date = DateTime.Today, Description = "" };
        db.Trips.AddRange(trip1, trip2);
        await db.SaveChangesAsync();

        await AttachTagsToTrip(db, trip1, "Jeseníky-test");
        await AttachTagsToTrip(db, trip2, "Jeseníky-test");

        // V DB smí být tag se slugem "jeseniky-test" pouze jednou
        db.Tags.Count(t => t.Slug == tagSlug).Should().Be(1,
            because: "duplicitní tag nesmí být vytvořen");
    }

    // ── prázdný seznam tagů neudělá chybu ────────────────────────────────────

    [Fact]
    public async Task SaveTags_EmptyTagNames_TripHasNoTags()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var trip = new Trip { Title = "Trip bez tagů", Date = DateTime.Today, Description = "" };
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        await AttachTagsToTrip(db, trip, "");

        var saved = await db.Trips.Include(t => t.Tags).FirstAsync(t => t.Id == trip.Id);
        saved.Tags.Should().BeEmpty(because: "prázdný TagNames = žádné tagy");
    }

    // ── null TagNames neudělá chybu ───────────────────────────────────────────

    [Fact]
    public async Task SaveTags_NullTagNames_TripHasNoTags()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var trip = new Trip { Title = "Trip null tagy", Date = DateTime.Today, Description = "" };
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        await AttachTagsToTrip(db, trip, null!);

        var saved = await db.Trips.Include(t => t.Tags).FirstAsync(t => t.Id == trip.Id);
        saved.Tags.Should().BeEmpty();
    }

    // ── duplicitní tagy ve vstupu se deduplikují ──────────────────────────────

    [Fact]
    public async Task SaveTags_DuplicateTagsInInput_OnlyOneTagCreated()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var trip = new Trip { Title = "Trip dup tagy", Date = DateTime.Today, Description = "" };
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        // Stejný tag třikrát (case-insensitive)
        await AttachTagsToTrip(db, trip, "UniqueTagABC, uniquetagabc, UNIQUETAGABC");

        var saved = await db.Trips.Include(t => t.Tags).FirstAsync(t => t.Id == trip.Id);
        saved.Tags.Should().HaveCount(1,
            because: "duplicitní tagy (case-insensitive) se musí sloučit do jednoho");
    }

    // ── slug je generován z názvu tagu ────────────────────────────────────────

    [Fact]
    public async Task SaveTags_TagWithDiacritics_SlugIsNormalized()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var trip = new Trip { Title = "Trip diakritika", Date = DateTime.Today, Description = "" };
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        await AttachTagsToTrip(db, trip, "Šumava-slug-test");

        var saved = await db.Trips.Include(t => t.Tags).FirstAsync(t => t.Id == trip.Id);
        saved.Tags.Should().Contain(t => t.Slug == "sumava-slug-test",
            because: "diakritika musí být normalizována ve slugu");
    }
}
