using System.IO;
using System.Windows;
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

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.Show();
    }

    private static void ConfigureServices(IServiceCollection services, IAppPaths appPaths)
    {
        services.AddSingleton(appPaths);

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={appPaths.DatabaseFilePath}"));

        services.AddSingleton<ICampaignService, CampaignService>();

        services.AddSingleton<ViewModels.MainWindowViewModel>();
        services.AddSingleton<MainWindow>();
    }

    /// <summary>
    /// Dev-time layout: walk up from the build output folder to the repo root (marked by the
    /// solution file) so runtime data lives at &lt;repo&gt;/Data regardless of build configuration.
    /// A future installed/published build can swap this for a fixed relative path instead.
    /// </summary>
    private static string ResolveDataRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null &&
               directory.GetFiles("GMHelper.slnx").Length == 0 &&
               directory.GetFiles("GMHelper.sln").Length == 0)
        {
            directory = directory.Parent;
        }

        var repoRoot = directory?.FullName ?? AppContext.BaseDirectory;
        return Path.Combine(repoRoot, "Data");
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        Log.CloseAndFlush();
        base.OnExit(e);
    }
}
