using Ganss.Xss;
using Microsoft.Extensions.DependencyInjection;
using TravelGallery.Data;
using TravelGallery.Models;
using TravelGallery.Tests.Fixtures;

namespace TravelGallery.Tests.Integration;

/// <summary>
/// Ověřuje, že HTML sanitizace funguje správně před uložením do DB
/// (stejná logika jako v Admin TripsController.Create a Edit).
/// </summary>
public class XssSanitizationTests : IClassFixture<AuthenticatedWebApplicationFactory>
{
    private readonly AuthenticatedWebApplicationFactory _factory;

    public XssSanitizationTests(AuthenticatedWebApplicationFactory factory)
    {
        _factory = factory;
    }

    // ── helper: vytvoř scope, sanitizuj a ulož ────────────────────────────────

    private async Task<Trip> CreateAndStoreTripWithDescription(string rawDescription)
    {
        using var scope = _factory.Services.CreateScope();
        var db        = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var sanitizer = scope.ServiceProvider.GetRequiredService<HtmlSanitizer>();

        var sanitized = sanitizer.Sanitize(rawDescription);

        var trip = new Trip
        {
            Title       = "XSS Test Trip " + Guid.NewGuid(),
            Date        = DateTime.Today,
            Description = sanitized
        };
        db.Trips.Add(trip);
        await db.SaveChangesAsync();

        return db.Trips.Find(trip.Id)!;
    }

    // ── script tag musí být odstraněn před uložením ───────────────────────────

    [Fact]
    public async Task CreateTrip_WithScriptInDescription_ScriptNotStoredInDb()
    {
        const string xss = "<p>Bezpečný obsah</p><script>alert('XSS')</script>";

        var stored = await CreateAndStoreTripWithDescription(xss);

        stored.Description.Should().NotContain("<script",
            because: "<script> tag nesmí být uložen do DB");
        stored.Description.Should().Contain("Bezpečný obsah",
            because: "bezpečný text musí být zachován");
    }

    // ── onerror atribut musí být odstraněn ────────────────────────────────────

    [Fact]
    public async Task CreateTrip_WithOnerrorAttribute_AttributeNotStoredInDb()
    {
        const string xss = "<img src='x' onerror='evil()'>";

        var stored = await CreateAndStoreTripWithDescription(xss);

        stored.Description.Should().NotContain("onerror=",
            because: "onerror event handler nesmí být uložen");
    }

    // ── iframe musí být odstraněn ────────────────────────────────────────────

    [Fact]
    public async Task CreateTrip_WithIframe_IframeNotStoredInDb()
    {
        const string xss = "<iframe src='https://evil.com'></iframe><p>Text</p>";

        var stored = await CreateAndStoreTripWithDescription(xss);

        stored.Description.Should().NotContain("<iframe",
            because: "iframe nesmí být uložen do DB");
        stored.Description.Should().Contain("Text",
            because: "bezpečný text musí zůstat");
    }

    // ── kombinovaný XSS payload ───────────────────────────────────────────────

    [Fact]
    public async Task CreateTrip_CombinedXssPayload_OnlySafeContentStored()
    {
        const string xss =
            "<p>Bezpečný text</p>" +
            "<script>alert('XSS_MARK')</script>" +
            "<img src='x' onerror='alert(2)'>" +
            "<iframe src='evil.com'></iframe>";

        var stored = await CreateAndStoreTripWithDescription(xss);

        stored.Description.Should().Contain("Bezpečný text");
        stored.Description.Should().NotContain("<script");
        stored.Description.Should().NotContain("XSS_MARK");
        stored.Description.Should().NotContain("onerror=");
        stored.Description.Should().NotContain("<iframe");
    }

    // ── prázdný popis projde bez chyby ───────────────────────────────────────

    [Fact]
    public async Task CreateTrip_EmptyDescription_StoredAsEmpty()
    {
        var stored = await CreateAndStoreTripWithDescription("");

        stored.Description.Should().BeEmpty();
    }

    // ── čisté HTML zůstane nezměněné ─────────────────────────────────────────

    [Fact]
    public async Task CreateTrip_SafeHtml_StoredUnchanged()
    {
        const string safe = "<p>Normální text s <strong>tučným</strong> písmem.</p>";

        var stored = await CreateAndStoreTripWithDescription(safe);

        stored.Description.Should().Contain("Normální text");
        stored.Description.Should().Contain("<strong>");
    }
}
