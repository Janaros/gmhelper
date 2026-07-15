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

    protected override void OnStartup(StartupEventArgs e)
    {
        RegisterSyncfusionLicense();

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
        _host.Services.GetRequiredService<Views.SecondaryDisplayWindow>();

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
    /// Reads a Syncfusion Community License key from a local, gitignored file at the repo root
    /// (syncfusion-license.local.txt) if present. Without it, Syncfusion controls run in an
    /// unlicensed trial state (dialog/watermark) instead of the app crashing.
    /// </summary>
    private static void RegisterSyncfusionLicense()
    {
        var licenseFilePath = Path.Combine(ResolveRepoRoot(), "syncfusion-license.local.txt");
        if (!File.Exists(licenseFilePath))
        {
            return;
        }

        var licenseKey = File.ReadAllText(licenseFilePath).Trim();
        if (!string.IsNullOrEmpty(licenseKey))
        {
            Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(licenseKey);
        }
    }

    /// <summary>
    /// Dev-time layout: walk up from the build output folder to the repo root (marked by the
    /// solution file) so runtime data lives at &lt;repo&gt;/Data regardless of build configuration.
    /// A future installed/published build can swap this for a fixed relative path instead.
    /// </summary>
    private static string ResolveDataRoot() => Path.Combine(ResolveRepoRoot(), "Data");

    private static string ResolveRepoRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null &&
               directory.GetFiles("GMHelper.slnx").Length == 0 &&
               directory.GetFiles("GMHelper.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? AppContext.BaseDirectory;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
