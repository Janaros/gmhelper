# GMHelper

WPF-Desktop-Anwendung für Game Master, die eine komplette Tabletop-RPG-Session (D&D 5e und andere Systeme) verwaltet: Kampagnen mit verknüpften PDFs (inkl. Stift-Annotationen direkt im PDF) und Bildern, ein spieler-sicheres Zweitbildschirm-Fenster, ein Spieler-Roster mit Initiative, eine systemunabhängige Monster-Datenbank und Markdown-Session-Notizen.

## Funktionsumfang

- **Kampagnen**: Verwaltung mehrerer Kampagnen mit eigener PDF- und Bild-Bibliothek.
- **PDFs mit Stift-Annotation**: PDFs werden in die App-eigene Bibliothek kopiert und können direkt mit Stift/Maus annotiert werden (Farbe, Dicke, Transparenz) — die Annotation wird echt ins PDF eingebrannt.
- **Zweitbildschirm**: Ein spieler-sicheres Fenster zeigt ausschließlich Bilder (Karten, NPCs, Monster) an, mit Blackout-Taste (F12) — strikt isoliert von GM-only-Daten wie Initiative oder HP.
- **Spieler-Roster**: Flexible, systemunabhängige Stat-Felder pro Spieler, Initiative als festes Feld.
- **Monster-Datenbank**: Globale, systemunabhängige Monster mit manuellem CRUD sowie JSON/CSV-Bulk-Import und -Export.
- **Kampf-Tracker**: Spieler und Monster-Instanzen (mit Auto-Nummerierung und editierbaren Kürzeln) in einen Encounter ziehen, Initiative auswürfeln (W20) oder eintragen, HP-Tracking, freie Zustands-Tags, Rundenzähler, "Nächster Zug".
- **Session-Notizen**: Markdown mit Live-Vorschau, chronologisch pro Kampagne.

## Installation

GMHelper wird per **ClickOnce** verteilt — Installation und spätere Updates laufen über einen einzigen Link, ohne separaten Installer-Download:

**➡ [GMHelper installieren](https://janaros.github.io/gmhelper/GMHelper.App.application)**

Hinweise:
- Die Anwendung ist **nicht code-signiert** (kein kostenpflichtiges Zertifikat vorhanden) — Windows zeigt beim ersten Start eine "Unbekannter Herausgeber"-Warnung. Das ist erwartet.
- GMHelper nutzt für die PDF-Anzeige/-Annotation eine kommerzielle Syncfusion-Komponente. Installierte Kopien laufen ohne eigenen Lizenzschlüssel dauerhaft im unlizenzierten Trial-Modus (Hinweisdialog/Wasserzeichen im PDF-Viewer), bleiben aber ansonsten voll funktionsfähig.
- Benötigt Windows 10/11 (x64). Die Anwendung ist self-contained und bringt die .NET-Runtime mit — es muss nichts separat installiert werden.
- ClickOnce-Updates: Beim nächsten Start wird automatisch auf eine neuere Version geprüft.

## Lokal bauen

Voraussetzungen: Windows, [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0).

```
git clone https://github.com/Janaros/gmhelper.git
cd gmhelper
dotnet build GMHelper.slnx
dotnet test GMHelper.slnx
dotnet run --project src/GMHelper.App/GMHelper.App.csproj
```

Beim ersten Start im Dev-Setup legt die App ihre Daten (SQLite-DB, kopierte PDFs/Bilder, Logs) unter `<Projektordner>\Data\` an. Ohne eigenen Syncfusion-Lizenzschlüssel läuft der PDF-Viewer im Trial-Modus (siehe oben).

Architektur, Konventionen und der vollständige Git-Workflow sind in [CLAUDE.md](CLAUDE.md) dokumentiert.

## ClickOnce-Release veröffentlichen

Für Maintainer mit Visual Studio (wird für das ClickOnce-Manifest-Tooling benötigt, siehe [CLAUDE.md](CLAUDE.md)):

```
pwsh scripts/Deploy-GhPages.ps1
```

Baut, published und deployed die aktuelle Version (`<Version>` in `GMHelper.App.csproj`, bei jeder Veröffentlichung hochzählen) auf den `gh-pages`-Branch.
