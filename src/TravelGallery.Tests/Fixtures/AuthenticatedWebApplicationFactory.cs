using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TravelGallery.Data;

namespace TravelGallery.Tests.Fixtures;

/// <summary>
/// Test factory s auto-přihlášením (TestAuthHandler) a in-memory DB.
/// Použij pro testy obsahu stránek, kde potřebuješ autentizovaného uživatele.
/// </summary>
public class AuthenticatedWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Přidej testovací konfiguraci (AdminSeed pro SeedData)
        builder.ConfigureAppConfiguration((_, cfg) =>
        {
            cfg.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["AdminSeed:Email"]       = "admin@test.com",
                ["AdminSeed:Password"]    = "Admin123!",
                ["AdminSeed:DisplayName"] = "Test Admin",
            });
        });

        builder.ConfigureServices(services =>
        {
            // Nahraď SQL Server in-memory DB.
            // EF Core 10 validuje, že pouze jeden provider je registrován.
            // Izolovaný internal service provider obchází sdílenou globální cache.
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);

            var dbName = "AuthTestDb_" + Guid.NewGuid();

            // Izolovaný EF Core InMemory provider – bez konfliktu se SQL Server
            var efInMemoryProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseInMemoryDatabase(dbName)
                    .UseInternalServiceProvider(efInMemoryProvider));

            // Nahraď auth schéma fake handlerem (vždy autentizuje)
            services.AddAuthentication(opts =>
            {
                opts.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                opts.DefaultChallengeScheme    = TestAuthHandler.SchemeName;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName, _ => { });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        // Seed testovacích dat po startu aplikace
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        TestDbSeeder.Seed(db);

        return host;
    }
}
