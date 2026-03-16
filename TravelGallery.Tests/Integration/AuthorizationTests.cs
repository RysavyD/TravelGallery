using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using TravelGallery.Tests.Fixtures;

namespace TravelGallery.Tests.Integration;

/// <summary>
/// Ověřuje, že chráněné endpointy správně odmítají anonymní přístupy.
/// Používá AnonymousWebApplicationFactory (bez fake autentizace).
/// </summary>
public class AuthorizationTests : IClassFixture<AnonymousWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthorizationTests(AnonymousWebApplicationFactory factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false  // Chceme vidět 302, ne výslednou stránku
        });
    }

    // ── admin area vyžaduje přihlášení ────────────────────────────────────────

    [Theory]
    [InlineData("/Admin/Trips/Index")]
    [InlineData("/Admin/Trips/Create")]
    [InlineData("/Admin/Users/Index")]
    public async Task AdminEndpoints_AnonymousUser_RedirectsToLogin(string url)
    {
        var response = await _client.GetAsync(url);

        response.StatusCode.Should().Be(HttpStatusCode.Redirect,
            because: $"nepřihlášený uživatel musí být přesměrován, URL: {url}");

        response.Headers.Location!.ToString()
            .Should().Contain("/Account/Login",
            because: $"přesměrování musí vést na login stránku, URL: {url}");
    }

    // ── veřejný seznam výletů také vyžaduje přihlášení ───────────────────────

    [Fact]
    public async Task PublicTripsIndex_AnonymousUser_RedirectsToLogin()
    {
        var response = await _client.GetAsync("/Trips/Index");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Contain("/Account/Login");
    }

    // ── login stránka je veřejná ──────────────────────────────────────────────

    [Fact]
    public async Task LoginPage_AnonymousUser_ReturnsOk()
    {
        var response = await _client.GetAsync("/Account/Login");

        response.StatusCode.Should().Be(HttpStatusCode.OK,
            because: "login stránka musí být dostupná bez přihlášení");
    }

    // ── returnUrl je předán při přesměrování na login ────────────────────────

    [Fact]
    public async Task AdminEndpoint_AnonymousUser_RedirectContainsReturnUrl()
    {
        var response = await _client.GetAsync("/Admin/Trips/Index");

        var location = response.Headers.Location!.ToString();
        // ASP.NET Identity generuje parametr "ReturnUrl" (s velkým R a U)
        location.Should().Contain("ReturnUrl",
            because: "redirect na login musí zachovat původní URL pro návrat");
    }
}
