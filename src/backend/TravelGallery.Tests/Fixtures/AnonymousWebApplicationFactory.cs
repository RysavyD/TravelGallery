using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TravelGallery.Data;

namespace TravelGallery.Tests.Fixtures;

/// <summary>
/// Test factory BEZ fake autentizace – slouží pro testy autorizačního chování
/// (ověření, že anonymní přístupy jsou správně odmítnuty/přesměrovány).
/// </summary>
public class AnonymousWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
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
            // Pouze nahraď DB – auth ponech původní (Identity cookie auth)
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor != null) services.Remove(descriptor);
            // EF Core 9+ drží konfiguraci UseSqlServer i v IDbContextOptionsConfiguration
            services.RemoveAll<IDbContextOptionsConfiguration<ApplicationDbContext>>();

            // Izolovaný EF Core InMemory provider
            var efInMemoryProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            services.AddDbContext<ApplicationDbContext>(opts =>
                opts.UseInMemoryDatabase("AnonTestDb_" + Guid.NewGuid())
                    .UseInternalServiceProvider(efInMemoryProvider));
        });
    }
}
