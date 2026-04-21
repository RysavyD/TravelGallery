# TravelGallery

Webová aplikace pro sdílení cestovatelských deníků s fotogalerií, mapami, EXIF metadaty a REST API pro mobilní klienty. Postavená na ASP.NET Core 10 MVC.

## Funkce

### Hlavní funkce
- **Vizuální časová osa** – chronologický přehled výletů s roky, měsíci a alternujícím rozložením; celá karta výletu klikatelná
- **Tmavý režim** – automatická detekce systémového nastavení + manuální přepínač s uložením preference
- **Fotogalerie** – lightbox (PhotoSwipe 5) se správným poměrem stran, drag & drop řazení v adminu, hromadný upload s progress barem
- **EXIF metadata** – automatická extrakce data, GPS, modelu fotoaparátu a expozice z nahraných fotek
- **Podpora HEIC/HEIF** – fotky z iPhonu se automaticky konvertují na JPEG (Magick.NET)
- **Komprese obrázků** – automatický resize na max 2560px při uploadu, generování thumbnailů
- **Automatické doplnění GPS** – pokud výlet nemá GPS, doplní se z první nahrané fotky s GPS

### Mapy
- **Detail výletu** – interaktivní mapa místa výletu (Leaflet.js + OpenStreetMap)
- **Mapa fotek** – u výletů s 10+ geotagovanými fotkami se zobrazí mapa s náhledy
- **Trasa výletu** – pokud mají fotky EXIF čas pořízení, na mapě se vykreslí spojnice mezi fotkami v chronologickém pořadí
- **Přehledová mapa** – samostatná stránka `/Trips/Map` se všemi výlety, které mají GPS

### Navigace a UX
- **Stránkování** – 20 výletů na stránku
- **Vyhledávání a filtrování** – fulltext, tagy, filtr podle data od–do
- **Navigace mezi výlety** – tlačítka „Starší" / „Novější" v detailu
- **Klik na miniaturu** – na úvodní stránce otevře galerii přímo na dané fotce

### Export a sdílení
- **Export do PDF** – tisk přes prohlížeč nebo serverový download (QuestPDF)
- **Zálohování** – admin může stáhnout kompletní ZIP (JSON + fotky) jedním kliknutím

### Správa a oprávnění
- **Skupiny uživatelů** – volitelně zapnutá funkce (`Features:GroupsEnabled`); každá skupina vidí jen svůj okruh výletů (M:N)
- **Přátelská stránka „nemáte přístup"** – místo 404 při nedostatečných právech
- **Počet zobrazení** – u výletů se počítají návštěvy (bez admina), viditelné v /Admin/Trips
- **Admin sekce** – správa výletů, fotek, uživatelů, skupin a hesel

### REST API
- **JWT autentizace** – access + refresh tokeny pro mobilního klienta
- **CRUD výletů a fotek** – `/api/trips`, `/api/trips/{id}/media`
- **Swagger UI** – dokumentace na `/swagger`
- **CORS** – povoleno pro mobilní aplikaci

## Tech stack

| Vrstva | Technologie |
|---|---|
| Backend | ASP.NET Core 10 MVC, EF Core 10, Identity, JWT Bearer |
| Databáze | SQL Server LocalDB |
| Frontend | Bootstrap 5.3.3, Quill 2, PhotoSwipe 5, Leaflet.js 1.9, SortableJS |
| Obrázky | ImageSharp 3.1 (thumbnaily, komprese), Magick.NET 14 (HEIC), MetadataExtractor 2.9 (EXIF) |
| PDF | QuestPDF 2024.12 |
| API dokumentace | Swashbuckle.AspNetCore 7 |

## Spuštění lokálně

### Požadavky

- .NET 10 SDK
- SQL Server LocalDB (součástí Visual Studio)

### Postup

```bash
git clone https://github.com/RysavyD/TravelGallery.git
cd TravelGallery
```

Vytvoř soubor `src/backend/TravelGallery/appsettings.Development.json` (není součástí repozitáře v produkční podobě):

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TravelGalleryDb;Trusted_Connection=True;MultipleActiveResultSets=true"
  },
  "AdminSeed": {
    "Email": "admin@example.com",
    "Password": "YourPassword123!",
    "DisplayName": "Admin"
  }
}
```

```bash
cd src/backend/TravelGallery
dotnet ef database update
dotnet run
```

Aplikace běží na **http://localhost:5117**, Swagger API dokumentace na **http://localhost:5117/swagger**.

## Struktura repozitáře

```
.
├── src/
│   ├── backend/                    # ASP.NET Core web + API
│   │   ├── TravelGallery/
│   │   │   ├── Areas/Admin/        # Admin sekce
│   │   │   ├── Controllers/        # Veřejné controllery + API (Api/)
│   │   │   ├── Data/               # EF DbContext, SeedData
│   │   │   ├── Migrations/         # EF Core migrace
│   │   │   ├── Models/             # Trip, Media, TravelGroup, Api DTOs
│   │   │   ├── Services/           # FileStorageService (upload, thumbnaily, EXIF, HEIC)
│   │   │   ├── ViewModels/         # ViewModely
│   │   │   ├── Views/              # Razor views
│   │   │   └── wwwroot/            # Statické soubory, uploady
│   │   ├── TravelGallery.Tests/    # xUnit testy
│   │   └── TravelGallery.slnx
│   └── mobile/                     # React Native admin app (plánovaná)
├── doc/                            # Dokumentace
├── README.md
└── .gitignore
```

## Role a oprávnění

| Role | Oprávnění |
|---|---|
| `Admin` | Plný přístup včetně `/Admin/` oblasti, správa uživatelů, skupin a záloh |
| `User` | Čtení timeline, detail výletu (filtrováno skupinami pokud jsou zapnuté) |

Admin účet se vytvoří automaticky při prvním spuštění ze sekce `AdminSeed` v konfiguraci.

## Konfigurace

### Feature flags (`appsettings.json`)

```json
"Features": {
  "GroupsEnabled": true
}
```

- `GroupsEnabled: true` – uživatelé vidí jen výlety ze skupin, kterých jsou členy
- `GroupsEnabled: false` – každý přihlášený uživatel vidí všechny výlety (role `Admin` / `User` jen pro správu)

### JWT (pro mobilní API)

```json
"Jwt": {
  "Key": "secret-key-min-32-chars",
  "Issuer": "TravelGallery",
  "Audience": "TravelGalleryMobile",
  "AccessTokenExpirationMinutes": 30,
  "RefreshTokenExpirationDays": 7
}
```

## Databázové schéma

Všechny tabulky jsou ve schématu `[travel]`.

```
Trip ──< Media
 │
 ├──< TripGroup >──< TravelGroup >──< UserGroup >── ApplicationUser
 │
 └──< TripTag >──< Tag

ApplicationUser ──< RefreshToken
```

## Licence

MIT
