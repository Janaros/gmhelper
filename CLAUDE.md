# GMHelper

WPF-Desktop-Anwendung für Game Master, die eine komplette Tabletop-RPG-Session (D&D 5e und andere Systeme) verwaltet: Kampagnen mit verknüpften PDFs (inkl. Stift-Annotationen direkt im PDF) und Bildern, ein spieler-sicheres Zweitbildschirm-Fenster, Spieler-Roster mit Initiative, eine systemunabhängige Monster-Datenbank und Markdown-Session-Notizen.

Der vollständige Architektur- und Phasenplan liegt (bis zur Umsetzung aller Phasen) als Referenz in der ursprünglichen Planungs-Session; die verbindlichen Konventionen unten gelten ab sofort für jede Änderung an diesem Repo.

## Coding-Vorgabe: SOLID

Jede Änderung an diesem Projekt hält sich an die SOLID-Prinzipien:

- **SRP**: Eine Service-Klasse hat genau eine Verantwortung (z.B. `CampaignService` kennt nur Kampagnen-CRUD, keine PDF- oder Bildlogik).
- **OCP**: Neue Funktionalität wird über neue Klassen/Implementierungen ergänzt, nicht durch Aufweichen bestehender Klassen mit Sonderfall-Flags.
- **LSP**: Austauschbare Implementierungen (z.B. `AppPaths` in Produktion vs. im Test mit Temp-Verzeichnis) müssen sich für Aufrufer identisch verhalten.
- **ISP**: Interfaces bleiben schmal und zweckgebunden (z.B. `ICampaignService`, `IAppPaths`) statt einer großen "Gott-Schnittstelle".
- **DIP**: ViewModels und Services hängen ausschließlich von Interfaces aus `GMHelper.Core.Abstractions` ab, nie von konkreten Implementierungen. Auch Umgebungsdetails wie Datenpfade werden injiziert (`IAppPaths`), nicht statisch/hart codiert — genau das macht Services ohne echtes UI oder echten Dateisystem-Mount testbar.

## Projektstruktur

```
GMHelper.slnx
  src/
    GMHelper.Core/        POCO-Entities, Enums, Service-Interfaces (Abstractions/), DTOs.
                           Keine WPF-/EF-Referenzen — reines Domain-Modell.
    GMHelper.Data/        AppDbContext, EF-Configurations, Migrations.
    GMHelper.Services/    Implementierungen der Core-Interfaces (CampaignService, AppPaths, ...).
    GMHelper.App/         WPF: App.xaml.cs (DI-Host/Composition Root), Views/, ViewModels/.
  tests/
    GMHelper.Core.Tests/        Unit-Tests, reine Logik ohne DB/Dateisystem.
    GMHelper.Services.Tests/    Unit-Tests für Service-Logik mit gemockten Abhängigkeiten.
    GMHelper.IntegrationTests/  Tests gegen eine echte temporäre SQLite-Datei + echtes
                                 Temp-Verzeichnis (kein Mocking von DbContext/Dateisystem hier —
                                 das Zusammenspiel EF-Mapping/Datei-Kopie/Ordner-Layout ist genau
                                 der Risikobereich, den diese Tests absichern).
```

Referenzrichtung: `Core` ← `Data` ← `Services` ← `App` (Core hat keine Abhängigkeiten, App kennt alle). ViewModels injizieren ausschließlich Interfaces aus `Core.Abstractions`.

## Datenablage

Alle Nutzdaten (SQLite-DB, kopierte PDFs/Bilder, Logs) liegen unter `<Projektordner>\Data\` — **bewusst im Projektordner, nicht unter dem Windows-Benutzerprofil `%LocalAppData%`**. Der Ordner ist per `.gitignore` vom Repo ausgeschlossen. Der Pfad wird zur Laufzeit über `IAppPaths` (Interface in `Core.Abstractions`, Implementierung `AppPaths` in `Services`) aufgelöst; `App.xaml.cs` bestimmt als Composition Root beim Start den Datenordner relativ zur Solution-Datei und registriert eine konkrete `IAppPaths`-Instanz in der DI-Container. Tests konstruieren `AppPaths` stattdessen mit einem temporären Verzeichnis — deshalb darf `AppPaths` nie statisch/global zugreifen, sondern nimmt den Root-Pfad im Konstruktor entgegen.

## Git-Workflow

- `main` ist der geschützte Hauptbranch. Jede inhaltliche Änderung läuft über einen eigenen **Feature-Branch** (`feature/<kurzname>`), nie direkt auf `main`.
- Ablauf pro Änderung: Branch erstellen → implementieren → `dotnet build` grün → `dotnet test` grün (Unit- **und** Integrationstests) → manueller Funktionstest in der laufenden App → erst dann committen (aussagekräftige Commit-Message) → Merge nach `main`.
- Kein Commit/Merge bei rotem Build- oder Test-Stand.
- Je Service/Bereich mindestens ein Integrationstest gegen eine echte temporäre SQLite-Datei + Temp-Ordner (siehe `GMHelper.IntegrationTests`).

## Build & Test

```
dotnet build GMHelper.slnx
dotnet test GMHelper.slnx
```

EF-Core-Migrationen (Startprojekt ist `GMHelper.App`, da dort die Design-Time-Factory-Abhängigkeit liegt):

```
dotnet ef migrations add <Name> --project src/GMHelper.Data --startup-project src/GMHelper.App --output-dir Migrations
```
