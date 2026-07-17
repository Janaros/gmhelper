using System.IO;
using System.Windows;
using CommunityToolkit.Mvvm.Messaging;
using GMHelper.App.Windows;
using GMHelper.Core.Abstractions;
using GMHelper.Data;
using GMHelper.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace GMHelper.App;

public partial class App : Application
{
    private IHost? _host;
    private Views.SecondaryDisplayWindow? _secondaryDisplayWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        RegisterSyncfusionLicense();

        // Must be set before any Window is constructed so standard WPF controls (Button,
        // TabControl, ListBox, ...) pick up the theme too, not just Syncfusion controls.
        Syncfusion.SfSkinManager.SfSkinManager.ApplyThemeAsDefaultStyle = true;
        Syncfusion.SfSkinManager.SfSkinManager.ApplicationTheme = new Syncfusion.SfSkinManager.Theme("Windows11Light");

        var appPaths = new AppPaths(ResolveDataRoot());
        Directory.CreateDirectory(appPaths.DataRoot);
        Directory.CreateDirectory(appPaths.LogsFolder);

        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(
                Path.Combine(appPaths.LogsFolder, "gmhelper-.log"),
                rollingInterval: RollingInterval.Day)
            .CreateLogger();

        _host = Host.CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureServices((_, services) => ConfigureServices(services, appPaths))
            .Build();

        base.OnStartup(e);

        using (var db = _host.Services.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext())
        {
            db.Database.Migrate();
        }

        // Created once at startup so its window position/blackout state persists for the
        // whole session; it stays hidden until the GM opens it via IWindowManager.
        _secondaryDisplayWindow = _host.Services.GetRequiredService<Views.SecondaryDisplayWindow>();

        ShutdownMode = ShutdownMode.OnMainWindowClose;

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services, IAppPaths appPaths)
    {
        services.AddSingleton(appPaths);

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={appPaths.DatabaseFilePath}"));

        services.AddSingleton<IMessenger>(WeakReferenceMessenger.Default);

        services.AddSingleton<ICampaignService, CampaignService>();
        services.AddSingleton<IPdfLibraryService, PdfLibraryService>();
        services.AddSingleton<IImageLibraryService, ImageLibraryService>();
        services.AddSingleton<IPlayerService, PlayerService>();
        services.AddSingleton<IStatFieldService, StatFieldService>();
        services.AddSingleton<IMonsterService, MonsterService>();
        services.AddSingleton<IMonsterImportService, MonsterImportService>();
        services.AddSingleton<IMonsterExportService, MonsterExportService>();
        services.AddSingleton<ICombatTrackerService, CombatTrackerService>();
        services.AddSingleton<ISessionNotesService, SessionNotesService>();
        services.AddSingleton<ICampaignExportService, CampaignExportService>();

        services.AddSingleton<ViewModels.ICampaignDetailViewModelFactory, ViewModels.CampaignDetailViewModelFactory>();
        services.AddSingleton<ViewModels.CampaignListViewModel>();
        services.AddSingleton<ViewModels.MonsterDatabaseViewModel>();
        services.AddSingleton<ViewModels.ShellViewModel>();
        services.AddSingleton<MainWindow>();

        services.AddSingleton<ViewModels.SecondaryDisplayViewModel>();
        services.AddSingleton<Views.SecondaryDisplayWindow>();
        services.AddSingleton<IWindowManager, WindowManager>();
    }

    /// <summary>
    /// Registers the Syncfusion Community License key, looked up in this order:
    /// 1. Dev workflow: local, gitignored syncfusion-license.local.txt at the repo root —
    ///    keeps the key out of the public repo.
    /// 2. Installed copies: an embedded resource that Build-InnoInstaller.ps1 injects at
    ///    packaging time from that same local file (never present in normal dev builds).
    /// Without either source, Syncfusion controls run in an unlicensed trial state
    /// (dialog/watermark) instead of the app crashing.
    /// </summary>
    private static void RegisterSyncfusionLicense()
    {
        var licenseKey = TryReadLicenseFromRepoRoot() ?? TryReadEmbeddedLicense();
        if (!string.IsNullOrEmpty(licenseKey))
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
        }
    }

    private static string? TryReadLicenseFromRepoRoot()
    {
        var repoRoot = TryFindRepoRoot();
        if (repoRoot is null)
        {
            return null;
        }

        var licenseFilePath = Path.Combine(repoRoot, "syncfusion-license.local.txt");
        if (!File.Exists(licenseFilePath))
        {
            return null;
        }

        var licenseKey = File.ReadAllText(licenseFilePath).Trim();
        return licenseKey.Length == 0 ? null : licenseKey;
    }

    private static string? TryReadEmbeddedLicense()
    {
        using var stream = typeof(App).Assembly.GetManifestResourceStream("GMHelper.App.SyncfusionLicense");
        if (stream is null)
        {
            return null;
        }

        using var reader = new StreamReader(stream);
        var licenseKey = reader.ReadToEnd().Trim();
        return licenseKey.Length == 0 ? null : licenseKey;
    }

    /// <summary>
    /// Dev-time layout: walk up from the build output folder to the repo root (marked by the
    /// solution file) so runtime data lives at &lt;repo&gt;/Data regardless of build configuration.
    /// Installed copies (e.g. ClickOnce) have no solution file to anchor to, and ClickOnce
    /// re-deploys each version into a fresh per-version cache folder, so anchoring to
    /// AppContext.BaseDirectory there would silently orphan the user's data on every update.
    /// Those copies get a stable per-user folder instead.
    /// </summary>
    private static string ResolveDataRoot()
    {
        var repoRoot = TryFindRepoRoot();
        if (repoRoot is not null)
        {
            return Path.Combine(repoRoot, "Data");
        }

        var localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        return Path.Combine(localAppData, "GMHelper");
    }

    private static string? TryFindRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (directory.GetFiles("GMHelper.slnx").Length > 0 || directory.GetFiles("GMHelper.sln").Length > 0)
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        return null;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _secondaryDisplayWindow?.SaveCurrentPlacement();
        _host?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
