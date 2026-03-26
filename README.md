# TravelGallery

Webová aplikace pro sdílení cestovatelských deníků s fotogalerií, mapami a EXIF metadaty. Postavená na ASP.NET Core 10 MVC.

## Funkce

- **Vizuální časová osa** – chronologický přehled výletů s roky, měsíci a alternujícím rozložením
- **Tmavý režim** – automatická detekce systémového nastavení + manuální přepínač s uložením preference
- **Fotogalerie** – lightbox (PhotoSwipe 5), drag & drop řazení v adminu, hromadný upload s progress barem
- **EXIF metadata** – automatická extrakce data, GPS, modelu fotoaparátu a expozice z nahraných fotek
- **Mapa fotek** – u výletů s 10+ geotagovanými fotkami se zobrazí mapa s náhledy
- **Mapy** – interaktivní mapa výletu (Leaflet.js + OpenStreetMap), přehledová mapa výletů na homepage
- **Komprese obrázků** – automatický resize na max 2560px při uploadu, generování thumbnailů
- **Export do PDF** – tisk přes prohlížeč nebo serverový download (QuestPDF)
- **Skupiny uživatelů** – každá skupina vidí jen svůj okruh výletů (M:N vazba)
- **Vyhledávání a filtrování** – fulltext, tagy, filtr podle data od–do
- **Stránkování** – 20 výletů na stránku
- **Admin sekce** – správa výletů, fotek, uživatelů, skupin a hesel

## Tech stack

| Vrstva | Technologie |
|---|---|
| Backend | ASP.NET Core 10 MVC, EF Core 10, Identity |
| Databáze | SQL Server LocalDB |
| Frontend | Bootstrap 5.3.3, Quill 2, PhotoSwipe 5, Leaflet.js 1.9, SortableJS |
| Obrázky | ImageSharp 3.1 (thumbnaily, komprese), MetadataExtractor 2.9 (EXIF) |
| PDF | QuestPDF 2024.12 |

## Spuštění lokálně

### Požadavky

- .NET 10 SDK
- SQL Server LocalDB (součástí Visual Studio)

### Postup

```bash
git clone https://github.com/RysavyD/TravelGallery.git
cd TravelGallery
```

Vytvoř soubor `TravelGallery/appsettings.Development.json` (není součástí repozitáře v produkční podobě):

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
cd TravelGallery
dotnet ef database update
dotnet run
```

Aplikace běží na **http://localhost:5117**.

## Struktura projektu

```
TravelGallery/
├── Areas/Admin/          # Admin sekce (výlety, média, uživatelé, skupiny)
├── Controllers/          # Veřejná část (timeline, detail výletu, účet)
├── Data/                 # EF DbContext, SeedData
├── Migrations/           # EF Core migrace
├── Models/               # Trip, Media, TravelGroup, ApplicationUser
├── Services/             # FileStorageService (upload, thumbnaily, EXIF)
├── ViewModels/           # ViewModely pro views
├── Views/                # Razor views
└── wwwroot/              # Statické soubory, uploady
```

## Role

| Role | Oprávnění |
|---|---|
| `Admin` | Plný přístup včetně `/Admin/` oblasti, správa uživatelů a skupin |
| `User` | Čtení timeline, detail výletu |

Admin účet se vytvoří automaticky při prvním spuštění ze sekce `AdminSeed` v konfiguraci.

## Databázové schéma

Všechny tabulky jsou ve schématu `[travel]`.

```
Trip ──< Media
 │
 └──< TripGroup >──< TravelGroup >──< UserGroup >── ApplicationUser
```

## Licence

MIT
