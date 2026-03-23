# TravelGallery

Webová aplikace pro sdílení cestovatelských deníků s fotogalerií, mapami a komentáři. Postavená na ASP.NET Core 10 MVC.

## Funkce

- **Timeline výletů** – chronologický přehled cest s tagy a fulltextovým vyhledáváním
- **Fotogalerie** – lightbox (PhotoSwipe 5), drag & drop řazení v adminu, hromadný upload s progress barem
- **Mapy** – interaktivní mapa výletu (Leaflet.js + OpenStreetMap), výběr souřadnic v adminu
- **Mapa cestovatele** – přehledová mapa všech výletů na homepage
- **Export do PDF** – tisk nebo serverový download (QuestPDF)
- **Skupiny uživatelů** – každá skupina vidí jen svůj okruh výletů (M:N vazba)
- **Vyhledávání** – fulltextové filtrování v horní liště
- **Tagy** – kategorizace výletů s filtrováním
- **Komentáře** – pro přihlášené uživatele
- **Admin sekce** – správa výletů, fotek, uživatelů a skupin

## Tech stack

| Vrstva | Technologie |
|---|---|
| Backend | ASP.NET Core 10 MVC, EF Core 10, Identity |
| Databáze | SQL Server LocalDB |
| Frontend | Bootstrap 5, Quill 2, PhotoSwipe 5, Leaflet.js 1.9, SortableJS |
| Obrázky | ImageSharp 3.1 (thumbnaily, resize) |
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
    "DefaultConnection": "Server=(localdb)\mssqllocaldb;Database=TravelGalleryDb;Trusted_Connection=True;MultipleActiveResultSets=true"
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
├── Models/               # Trip, Media, Comment, TravelGroup, ApplicationUser
├── Services/             # FileStorageService (upload + thumbnaily)
├── ViewModels/           # ViewModely pro views
├── Views/                # Razor views
└── wwwroot/              # Statické soubory, uploady
```

## Role

| Role | Oprávnění |
|---|---|
| `Admin` | Plný přístup včetně `/Admin/` oblasti |
| `User` | Čtení timeline, detail výletu, komentáře |

Admin účet se vytvoří automaticky při prvním spuštění ze sekce `AdminSeed` v konfiguraci.

## Databázové schéma

Všechny tabulky jsou ve schématu `[travel]`.

```
Trip ──< Media ──< Comment
 │
 └──< TripGroup >──< TravelGroup >──< UserGroup >── ApplicationUser
```

## Licence

MIT
