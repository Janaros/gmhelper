# GMHelper

WPF-Desktop-Anwendung für Game Master, die eine komplette Tabletop-RPG-Session (D&D 5e und andere Systeme) verwaltet: Kampagnen mit verknüpften PDFs (inkl. Stift-Annotationen direkt im PDF) und Bildern, ein spieler-sicheres Zweitbildschirm-Fenster, ein Spieler-Roster mit Initiative, eine systemunabhängige Monster-Datenbank, eine Kräuterkunde-Suche mit Sammelgebieten und Zutatentabellen sowie Markdown-Session-Notizen.

## Funktionsumfang

- **Kampagnen**: Verwaltung mehrerer Kampagnen mit eigener PDF- und Bild-Bibliothek.
- **PDFs mit Stift-Annotation**: PDFs werden in die App-eigene Bibliothek kopiert und können direkt mit Stift/Maus annotiert werden (Farbe, Dicke, Transparenz) — die Annotation wird echt ins PDF eingebrannt.
- **Zweitbildschirm**: Ein spieler-sicheres Fenster zeigt ausschließlich Bilder (Karten, NPCs, Monster) an, mit Blackout-Taste (F12) — strikt isoliert von GM-only-Daten wie Initiative oder HP.
- **Spieler-Roster**: Flexible, systemunabhängige Stat-Felder pro Spieler, Initiative als festes Feld.
- **Monster-Datenbank**: Globale, systemunabhängige Monster mit manuellem CRUD sowie JSON/CSV-Bulk-Import und -Export.
- **Kampf-Tracker**: Spieler und Monster-Instanzen (mit Auto-Nummerierung und editierbaren Kürzeln) in einen Encounter ziehen, Initiative auswürfeln (W20) oder eintragen, HP-Tracking, freie Zustands-Tags, Rundenzähler, "Nächster Zug".
- **Kräuterkunde**: Sammelgebiete der Schwertküste mit je eigener Fundtabelle für Trank- und Zauberzutaten (Art, Seltenheit, Wirkung, Wert), durchsuch- und filterbar. Der Sammelwurf würfelt Weisheit (Überleben) gegen den SG des Gebiets — je 5 Punkte über dem SG gibt es eine Zutat mehr und schaltet seltenere frei, das Kräuterkundeset gibt Vorteil. Gebiete und Zutaten sind frei editierbar; die Schwertküsten-Startdaten werden beim ersten Öffnen angelegt.
- **Session-Notizen**: Markdown mit Live-Vorschau, chronologisch pro Kampagne.

## Installation

**➡ [Neueste Version herunterladen](https://github.com/Janaros/gmhelper/releases/latest)**

Installer herunterladen und ausführen — kein Adminrecht nötig, installiert nur für den aktuellen Benutzer (`%LocalAppData%\Programs\GMHelper`), legt eine Startmenü-Verknüpfung an und lässt sich über "Apps & Features" wieder sauber deinstallieren.

Hinweise:
- Die Anwendung ist **nicht code-signiert** (kein kostenpflichtiges Zertifikat vorhanden) — Windows zeigt beim ersten Start eine "Unbekannter Herausgeber"-Warnung (SmartScreen). Das ist erwartet.
- GMHelper nutzt für die PDF-Anzeige/-Annotation eine kommerzielle Syncfusion-Komponente (Community License). Offizielle Installer-Builds sind lizenziert; nur selbst gebaute Kopien ohne eigenen Lizenzschlüssel laufen im Trial-Modus (Hinweisdialog/Wasserzeichen im PDF-Viewer), bleiben aber voll funktionsfähig.
- Benötigt Windows 10/11 (x64). Die Anwendung ist self-contained und bringt die .NET-Runtime mit — es muss nichts separat installiert werden.
- Updates: Es gibt aktuell kein Auto-Update — für eine neue Version die aktuelle `GMHelper-Setup-*.exe` von der [Releases-Seite](https://github.com/Janaros/gmhelper/releases) herunterladen und erneut ausführen (überschreibt die vorhandene Installation).

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

## Installer veröffentlichen

Für Maintainer (benötigt [Inno Setup](https://jrsoftware.org/isinfo.php) und die [GitHub CLI](https://cli.github.com/)):

```
pwsh scripts/Build-InnoInstaller.ps1
gh release create vX.Y.Z dist/GMHelper-Setup-X.Y.Z.exe --title "GMHelper X.Y.Z" --notes "..."
```

`<Version>` in `GMHelper.App.csproj` bei jeder Veröffentlichung hochzählen (erscheint auch im About-Dialog).
