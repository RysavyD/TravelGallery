using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using TravelGallery.Tests.Fixtures;

namespace TravelGallery.Tests.Integration;

/// <summary>
/// Integrační testy pro vyhledávání a filtrování výletů.
/// Používá AuthenticatedWebApplicationFactory (auto-přihlášení).
/// Poznámka: response.Content je HTML s enkódovanými Czech znaky (&#xED; atd.),
/// proto každý test provede WebUtility.HtmlDecode() před asercemi.
/// </summary>
public class TripsSearchTests : IClassFixture<AuthenticatedWebApplicationFactory>
{
    private readonly HttpClient _client;

    public TripsSearchTests(AuthenticatedWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = true
        });
    }

    // ── helper ───────────────────────────────────────────────────────────────

    private static string Decode(string html) => WebUtility.HtmlDecode(html);

    // ── základní zobrazení ───────────────────────────────────────────────────

    [Fact]
    public async Task Index_NoQuery_ReturnsAllTrips()
    {
        var response = await _client.GetAsync("/Trips/Index");
        var content  = Decode(await response.Content.ReadAsStringAsync());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("Lysou horu",      because: "první trip musí být na stránce");
        content.Should().Contain("Zimní turistika",  because: "druhý trip musí být na stránce");
        content.Should().Contain("Cyklovýlet",       because: "třetí trip musí být na stránce");
    }

    // ── vyhledávání podle názvu ──────────────────────────────────────────────

    [Fact]
    public async Task Index_SearchByTitleSubstring_ReturnsMatchingTrips()
    {
        // Hledáme "Zimní" – URL-encodovaně jako %C5%BAmn%C3%AD
        var response = await _client.GetAsync("/Trips/Index?q=Zimn%C3%AD");
        var content  = Decode(await response.Content.ReadAsStringAsync());

        content.Should().Contain("turistika",  because: "search 'Zimní' musí najít trip 'Zimní turistika'");
        content.Should().NotContain("Lysou horu", because: "nesouvisející trip nesmí být ve výsledcích");
    }

    [Fact]
    public async Task Index_SearchCaseConsistent_FindsTrip()
    {
        // EF Core InMemory provádí case-sensitive porovnání – hledáme přesnou shodu
        var response = await _client.GetAsync("/Trips/Index?q=Lysou");
        var content  = Decode(await response.Content.ReadAsStringAsync());

        content.Should().Contain("Lysou horu", because: "'Lysou' je podřetězec titulku prvního tripu");
    }

    // ── vyhledávání podle tagu ───────────────────────────────────────────────

    [Fact]
    public async Task Index_FilterByTag_ReturnsOnlyTaggedTrips()
    {
        var response = await _client.GetAsync("/Trips/Index?tag=zima");
        var content  = Decode(await response.Content.ReadAsStringAsync());

        content.Should().Contain("turistika",  because: "trip se slugem 'zima' musí být ve výsledcích");
        content.Should().NotContain("Cyklovýlet", because: "cyklistický trip tag 'zima' nemá");
    }

    // ── kombinace search + tag ───────────────────────────────────────────────

    [Fact]
    public async Task Index_SearchAndTagCombined_ReturnsIntersection()
    {
        // tag=hory vrací Lysou horu + Zimní turistika
        // q=Zim filtruje jen Zimní turistika
        var response = await _client.GetAsync("/Trips/Index?tag=hory&q=Zim");
        var content  = Decode(await response.Content.ReadAsStringAsync());

        content.Should().Contain("turistika",  because: "trip má tag 'hory' a název obsahuje 'Zim'");
        content.Should().NotContain("Lysou horu", because: "Lysou horu název 'Zim' neobsahuje");
    }

    // ── prázdný výsledek ─────────────────────────────────────────────────────

    [Fact]
    public async Task Index_NoMatchingQuery_ShowsEmptyMessage()
    {
        var response = await _client.GetAsync("/Trips/Index?q=xyzNeniCoTakoveho999");
        var content  = Decode(await response.Content.ReadAsStringAsync());

        content.Should().NotContain("trip-card",
            because: "žádné výsledky → žádná karta výletu");
        content.Should().Contain("text-center text-muted py-5",
            because: "div s prázdnou zprávou musí být přítomen");
    }

    // ── badge s aktivním filtrem ──────────────────────────────────────────────

    [Fact]
    public async Task Index_WithSearchQuery_ShowsSearchBadge()
    {
        var response = await _client.GetAsync("/Trips/Index?q=Hory");
        var content  = Decode(await response.Content.ReadAsStringAsync());

        content.Should().Contain("Hory",  because: "badge musí zobrazit hledaný výraz");
        content.Should().Contain("Zru",   because: "tlačítko 'Zrušit filtr' musí být přítomno");
    }

    // ── q parametr se přenáší do tag odkazů ─────────────────────────────────

    [Fact]
    public async Task Index_WithSearchAndTagResults_TagLinksPreserveQuery()
    {
        // Hledáme trip, který má tagy – "Lysou" najde trip s tagem "hory"
        var response = await _client.GetAsync("/Trips/Index?q=Lysou");
        var content  = Decode(await response.Content.ReadAsStringAsync());

        content.Should().Contain("q=Lysou",
            because: "tag linky musí obsahovat q= parametr pro zachování vyhledávání");
    }

    // ── pořadí výletů (sestupně podle data) ──────────────────────────────────

    [Fact]
    public async Task Index_TripsOrderedByDateDescending()
    {
        var response = await _client.GetAsync("/Trips/Index");
        var content  = Decode(await response.Content.ReadAsStringAsync());

        // Cyklovýlet (srpen 2024) musí být na stránce před Zimní turistikou (leden 2024)
        var posCyklo = content.IndexOf("Cyklovýlet", StringComparison.Ordinal);
        var posZima  = content.IndexOf("turistika",  StringComparison.Ordinal);

        posCyklo.Should().BeGreaterThan(0, because: "'Cyklovýlet' musí být přítomen na stránce");
        posZima.Should().BeGreaterThan(0,  because: "'turistika' musí být přítomno na stránce");
        posCyklo.Should().BeLessThan(posZima,
            because: "novější výlety musí být zobrazeny dříve (sestupné pořadí)");
    }
}
