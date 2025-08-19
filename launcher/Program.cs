using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using BlazedOdysseyLauncher.Services;
using BlazedOdysseyLauncher.ViewModels;
using BlazedOdysseyLauncher.Views;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace BlazedOdysseyLauncher;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        var services = ConfigureServices();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private ServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();
        
        // Register services
        services.AddSingleton<IUpdateService, UpdateService>();
        services.AddSingleton<IDownloadService, DownloadService>();
        services.AddSingleton<IGameLaunchService, GameLaunchService>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IManifestService, ManifestService>();
        services.AddSingleton<ISecurityService, SecurityService>();
        
        // Register ViewModels
        services.AddTransient<MainWindowViewModel>();
        
        return services.BuildServiceProvider();
    }
}

public class Program
{
    [STAThread]
    public static async Task Main(string[] args)
    {
        try
        {
            // Initialize logging
            var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), 
                "BlazedOdyssey", "Launcher", "logs");
            Directory.CreateDirectory(logPath);
            
            // Setup global exception handling
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                var exception = e.ExceptionObject as Exception;
                File.AppendAllText(Path.Combine(logPath, "crash.log"), 
                    $"{DateTime.Now}: {exception?.ToString()}\n");
            };

            BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            // Fallback logging
            try
            {
                var logPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "launcher_crash.log");
                File.WriteAllText(logPath, $"Launcher failed to start: {ex}");
            }
            catch { }
            
            throw;
        }
    }

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();
}