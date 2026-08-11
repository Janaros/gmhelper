# GMHelper

WPF-Desktop-Anwendung für Game Master, die eine komplette Tabletop-RPG-Session (D&D 5e und andere Systeme) verwaltet: Kampagnen mit verknüpften PDFs (inkl. Stift-Annotationen direkt im PDF) und Bildern, ein spieler-sicheres Zweitbildschirm-Fenster, Spieler-Roster mit Initiative, eine systemunabhängige Monster-Datenbank, eine Kräuterkunde-Suche mit Sammelgebieten und Zutatentabellen sowie Markdown-Session-Notizen.

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

Diese Regel gilt für den **Dev-Workflow** (Repo mit `GMHelper.slnx`). Bei einer **installierten** Kopie (z.B. per ClickOnce) existiert keine Solution-Datei zum Verankern, und ClickOnce entpackt jede Version in einen neuen, versionsspezifischen Cache-Ordner — ein Fallback auf `AppContext.BaseDirectory` würde die Nutzdaten bei jedem Update verwaisen lassen. Findet `App.xaml.cs` beim Hochlaufen kein `GMHelper.slnx`/`.sln` in einem Vorgänger-Verzeichnis der exe, weicht es stattdessen auf einen stabilen, versionsunabhängigen Ordner unter `%LocalAppData%\GMHelper` aus (siehe `ResolveDataRoot()`/`TryFindRepoRoot()` in `App.xaml.cs`).

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

## UI-Konventionen: Abstände und Styles

Alle Abstands-/Größen-Defaults liegen zentral in `App.xaml` (`Application.Resources`), damit die
Ansichten nicht jede für sich 2/4/6/8/10/12 px erfinden — genau das ließ die App vorher unruhig
wirken. Views setzen deshalb **keine eigenen** `Padding`-Werte auf Buttons/TextBoxen mehr.

- **Abstandsskala** (als `Thickness`-Ressourcen): `ViewMargin` (16) für Ansichten direkt im Shell,
  `SubViewMargin` (12) für Ansichten in einem Tab (der Tab-Rahmen rückt sie schon ein),
  `GutterMargin` zwischen nebeneinander liegenden Spalten, `FieldMargin` unter einem Eingabefeld.
  Abstände zwischen Buttons einer Werkzeugleiste: `Margin="8,0,0,0"`, bei Gruppenwechsel `16`.
- **Implizite Styles** für `Button`/`ToggleButton`/`TextBox`/`ComboBox`/`ListBoxItem` setzen nur
  Abstand und Mindesthöhe, keine Templates oder Farben — das Aussehen bleibt Sache des
  Syncfusion-Themes. Sie stehen bewusst direkt in `Application.Resources` (nicht in einem
  `MergedDictionary`), weil lokale Einträge gegen Theme-Dictionaries gewinnen.
- **Benannte Styles** statt wiederholter Inline-Formatierung: `IconButtonStyle` (quadratische
  Symbol-Buttons, sonst schneidet das Standard-Padding den Glyph ab), `GridCellTextBoxStyle`
  (TextBox in einer Tabellenzelle), `PageTitleTextStyle`, `SectionHeaderTextStyle`,
  `FieldLabelTextStyle`, `HintTextStyle`, `StatusTextStyle`, `CardBorderStyle`.
- **Ein Klick statt zwei in Tabellen**: `SfDataGrid` beansprucht den ersten Linksklick in einer
  Zeile für die eigene Zellauswahl, eine `TextBox` in einer `GridTemplateColumn` bekäme den Fokus
  also erst beim zweiten Klick. `CombatTrackerView` fängt das über
  `CellTextBox_PreviewMouseLeftButtonDown` ab (Fokus + Cursorposition setzen, Klick als behandelt
  markieren). Neue editierbare Grid-Spalten übernehmen denselben Style/Handler.

## PDF-Engine: Syncfusion

PDF-Anzeige und Stift-Annotation laufen über `Syncfusion.PdfViewer.WPF` (`Syncfusion.Windows.PdfViewer.PdfViewerControl`). Bewusste Entscheidung: kommerzielle Community-License-Komponente statt einer lizenzfreien Alternative (siehe Abwägung in der Planungs-Session) — falls das Projekt kommerzialisiert wird und die Umsatz-/Entwicklerzahl-Grenze der Community License überschritten wird, muss auf eine kostenpflichtige Syncfusion-Lizenz oder eine der geprüften Alternativen (PDF.js/WebView2, natives WPF-InkCanvas + PDFium/PDFsharp) migriert werden.

Lizenzschlüssel wird **nicht** eingecheckt (das Repo ist öffentlich). `App.xaml.cs` sucht ihn in dieser Reihenfolge:

1. **Dev-Workflow**: `<Projektordner>\syncfusion-license.local.txt` (per `.gitignore` ausgeschlossen).
2. **Installierte Kopien**: eingebettete Assembly-Ressource `GMHelper.App.SyncfusionLicense` — `Build-InnoInstaller.ps1` übergibt beim Packaging `-p:SyncfusionLicenseFile=<Pfad zur lokalen Datei>`, wodurch eine bedingte `EmbeddedResource` in `GMHelper.App.csproj` greift. Normale Dev-/CI-Builds setzen die Property nie, dort wird nichts eingebettet.

Ohne beide Quellen läuft die App im unlizenzierten Trial-Modus (Hinweisdialog/Wasserzeichen), stürzt aber nicht ab. Bewusste Abwägung zum Einbetten: Der Syncfusion-Schlüssel ist kein Zugangs-Geheimnis (er schaltet nur Komponenten frei, gewährt keinen Kontozugriff) und das Einbetten in ausgelieferte Binaries ist Syncfusions vorgesehener Weg — nur im öffentlichen Quellcode darf er nicht landen. Dass er aus der exe theoretisch per Reverse-Engineering extrahierbar ist, ist akzeptiert und bei jeder Syncfusion-Desktop-App so.

## Installer / Distribution

GMHelper wird per klassischem **Inno-Setup-Installer** verteilt, veröffentlicht als Anhang eines **GitHub Release** unter `https://github.com/Janaros/gmhelper/releases`.

- Build/Package-Schritt: `scripts/Build-InnoInstaller.ps1` — `dotnet publish` (self-contained `win-x64`, `Release`, **kein** ClickOnce) nach `<Projektordner>\publish-inno\`, dann `ISCC.exe` (Inno-Setup-Compiler, `winget install JRSoftware.InnoSetup`) gegen `installer/GMHelper.iss`. Ergebnis: `<Projektordner>\dist\GMHelper-Setup-<Version>.exe` (beide Ordner gitignored, lokal generiert). Bettet dabei den Syncfusion-Lizenzschlüssel aus der lokalen Datei ein (siehe Abschnitt „PDF-Engine: Syncfusion“) — ohne die Datei baut der Installer trotzdem, die Kopien laufen dann im Trial-Modus.
- Der Installer läuft **ohne Adminrechte** (`PrivilegesRequired=lowest`, Ziel `%LocalAppData%\Programs\GMHelper`) — passend dazu, dass kein Code-Signing-Zertifikat vorhanden ist (Windows zeigt beim ersten Start trotzdem eine "Unbekannter Herausgeber"-Warnung, das ist erwartet und unvermeidbar ohne Zertifikat).
- Veröffentlichen: `gh release create vX.Y.Z dist/GMHelper-Setup-X.Y.Z.exe --title "..." --notes "..."` (GitHub CLI, `winget install GitHub.cli`, einmalig `gh auth login --web`).
- **Jede neu veröffentlichte Version braucht eine neue `<Version>`** in `GMHelper.App.csproj` (fließt automatisch in `AppVersion`/Dateiname des Installers ein) — deckt sich mit der bestehenden Vorgabe, die Versionsnummer bei jeder ausgelieferten Änderung zu erhöhen.
- `App.xaml.cs`s Datenordner-Fallback (`%LocalAppData%\GMHelper`, siehe Abschnitt „Datenablage“ oben) ist die Voraussetzung dafür, dass installierte Kopien bei jedem Update (neue Version über den Installer drüberinstalliert) ihre Daten behalten.
- Kein Auto-Update: Nutzer laden bei einer neuen Version die aktuelle `GMHelper-Setup-*.exe` erneut herunter und führen sie aus (überschreibt die bestehende Installation, gleiche `AppId` in `installer/GMHelper.iss` — **niemals ändern**, sonst erkennt Inno Setup künftige Versionen nicht mehr als Upgrade derselben Installation).

### Verworfen: ClickOnce

Der erste Anlauf nutzte **ClickOnce** (gehostet auf GitHub Pages, `gh-pages`-Branch, gebaut über das Full-Framework-MSBuild einer Visual-Studio-Installation, da `dotnet publish` die ClickOnce-Manifest-Tasks nicht unterstützt — `MSB4803: GenerateTrustInfo`). Hosting und Manifest-Hashes wurden verifiziert korrekt (u.a. ein CRLF-Bug behoben, bei dem Git die Zeilenenden der `.manifest`-Dateien normalisierte und damit ihren eingebetteten SHA-256-Hash brach — falls das je wieder auftaucht: Ursache ist `core.autocrlf`, Fix ist eine `.gitattributes` mit `* -text -diff` auf dem Hosting-Branch). Trotzdem schlug die clientseitige ClickOnce-Aktivierung (`System.Deployment.Application`) auf dem Entwicklungsrechner wiederholt mit Fehlern fehl, die nichts mit Hosting/Manifest zu tun hatten (u.a. `System.UriFormatException` beim Aktivieren aus einem lokalen Pfad mit Umlaut im Windows-Benutzerprofil) — dieser Fehlerpfad ist in der .NET-Framework-eigenen ClickOnce-Komponente kaum diagnostizierbar. Deshalb der Wechsel auf Inno Setup, das direkt den bereits erfolgreich getesteten self-contained Build verpackt, ohne über `System.Deployment` zu laufen. Die ClickOnce-Infrastruktur (`scripts/Publish-ClickOnce.ps1`, `scripts/Deploy-GhPages.ps1`, `src/GMHelper.App/Properties/PublishProfiles/ClickOnce.pubxml`, `gh-pages`-Branch) ist noch im Repo, wird aber nicht mehr verwendet.
