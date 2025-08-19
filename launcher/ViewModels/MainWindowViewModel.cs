using Avalonia.Threading;
using BlazedOdysseyLauncher.Models;
using BlazedOdysseyLauncher.Services;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace BlazedOdysseyLauncher.ViewModels;

public class MainWindowViewModel : ReactiveObject
{
    private readonly IUpdateService _updateService;
    private readonly IGameLaunchService _gameLaunchService;
    private readonly ISettingsService _settingsService;

    private string _currentVersion = "Unknown";
    private string _availableVersion = "Unknown";
    private string _status = "Ready";
    private bool _isUpdating = false;
    private bool _canPlay = true;
    private bool _canUpdate = false;
    private bool _canRepair = true;
    private double _progressValue = 0;
    private bool _isProgressIndeterminate = false;
    private string _progressText = "";
    private CancellationTokenSource? _cancellationTokenSource;
    private GameManifest? _availableManifest;

    public MainWindowViewModel(
        IUpdateService updateService,
        IGameLaunchService gameLaunchService,
        ISettingsService settingsService)
    {
        _updateService = updateService;
        _gameLaunchService = gameLaunchService;
        _settingsService = settingsService;

        // Commands
        CheckForUpdatesCommand = ReactiveCommand.CreateFromTask(CheckForUpdatesAsync);
        UpdateGameCommand = ReactiveCommand.CreateFromTask(UpdateGameAsync, this.WhenAnyValue(x => x.CanUpdate));
        PlayGameCommand = ReactiveCommand.CreateFromTask(PlayGameAsync, this.WhenAnyValue(x => x.CanPlay));
        RepairGameCommand = ReactiveCommand.CreateFromTask(RepairGameAsync, this.WhenAnyValue(x => x.CanRepair));
        CancelCommand = ReactiveCommand.Create(CancelCurrentOperation);
        OpenSettingsCommand = ReactiveCommand.Create(OpenSettings);

        // Event subscriptions
        _updateService.ProgressChanged += OnProgressChanged;
        _updateService.StatusChanged += OnStatusChanged;
        _updateService.ErrorOccurred += OnErrorOccurred;

        // Initialize
        _ = Task.Run(InitializeAsync);
    }

    public string Title => "Blazed Odyssey Launcher";
    public string CurrentVersion
    {
        get => _currentVersion;
        set => this.RaiseAndSetIfChanged(ref _currentVersion, value);
    }

    public string AvailableVersion
    {
        get => _availableVersion;
        set => this.RaiseAndSetIfChanged(ref _availableVersion, value);
    }

    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public bool IsUpdating
    {
        get => _isUpdating;
        set => this.RaiseAndSetIfChanged(ref _isUpdating, value);
    }

    public bool CanPlay
    {
        get => _canPlay;
        set => this.RaiseAndSetIfChanged(ref _canPlay, value);
    }

    public bool CanUpdate
    {
        get => _canUpdate;
        set => this.RaiseAndSetIfChanged(ref _canUpdate, value);
    }

    public bool CanRepair
    {
        get => _canRepair;
        set => this.RaiseAndSetIfChanged(ref _canRepair, value);
    }

    public double ProgressValue
    {
        get => _progressValue;
        set => this.RaiseAndSetIfChanged(ref _progressValue, value);
    }

    public bool IsProgressIndeterminate
    {
        get => _isProgressIndeterminate;
        set => this.RaiseAndSetIfChanged(ref _isProgressIndeterminate, value);
    }

    public string ProgressText
    {
        get => _progressText;
        set => this.RaiseAndSetIfChanged(ref _progressText, value);
    }

    public ObservableCollection<string> LogMessages { get; } = new();

    public ICommand CheckForUpdatesCommand { get; }
    public ICommand UpdateGameCommand { get; }
    public ICommand PlayGameCommand { get; }
    public ICommand RepairGameCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand OpenSettingsCommand { get; }

    private async Task InitializeAsync()
    {
        try
        {
            CurrentVersion = _updateService.GetCurrentVersion();
            
            var settings = _settingsService.LoadSettings();
            if (settings.CheckUpdatesOnStart)
            {
                await CheckForUpdatesAsync();
            }
        }
        catch (Exception ex)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                AddLogMessage($"Initialization error: {ex.Message}");
            });
        }
    }

    private async Task CheckForUpdatesAsync()
    {
        try
        {
            SetOperationState(true);
            _cancellationTokenSource = new CancellationTokenSource();

            _availableManifest = await _updateService.CheckForUpdatesAsync(_cancellationTokenSource.Token);

            if (_availableManifest != null)
            {
                AvailableVersion = _availableManifest.Version;
                CanUpdate = true;
                Status = $"Update available: {_availableManifest.Version}";
                AddLogMessage($"Update found: {_availableManifest.Version}");
            }
            else
            {
                AvailableVersion = CurrentVersion;
                CanUpdate = false;
                Status = "Game is up to date";
                AddLogMessage("Game is up to date");
            }
        }
        catch (OperationCanceledException)
        {
            Status = "Update check cancelled";
        }
        catch (Exception ex)
        {
            Status = $"Update check failed: {ex.Message}";
            AddLogMessage($"Update check error: {ex.Message}");
        }
        finally
        {
            SetOperationState(false);
        }
    }

    private async Task UpdateGameAsync()
    {
        if (_availableManifest == null)
            return;

        try
        {
            SetOperationState(true);
            _cancellationTokenSource = new CancellationTokenSource();

            var success = await _updateService.UpdateGameAsync(_availableManifest, _cancellationTokenSource.Token);

            if (success)
            {
                CurrentVersion = _availableManifest.Version;
                AvailableVersion = _availableManifest.Version;
                CanUpdate = false;
                Status = "Update completed successfully";
                AddLogMessage("Game updated successfully");
                
                var settings = _settingsService.LoadSettings();
                if (settings.AutoLaunch)
                {
                    await PlayGameAsync();
                }
            }
            else
            {
                Status = "Update failed";
                AddLogMessage("Update failed");
            }
        }
        catch (OperationCanceledException)
        {
            Status = "Update cancelled";
            AddLogMessage("Update cancelled by user");
        }
        catch (Exception ex)
        {
            Status = $"Update failed: {ex.Message}";
            AddLogMessage($"Update error: {ex.Message}");
        }
        finally
        {
            SetOperationState(false);
        }
    }

    private async Task PlayGameAsync()
    {
        try
        {
            SetOperationState(true, "Launching game...");

            var settings = _settingsService.LoadSettings();
            var success = await _gameLaunchService.LaunchGameAsync(settings.GamePath);

            if (success)
            {
                AddLogMessage("Game launched successfully");
                
                if (settings.CloseOnLaunch)
                {
                    Environment.Exit(0);
                }
                else
                {
                    Status = "Game is running";
                    // Monitor game process
                    _ = Task.Run(async () =>
                    {
                        await _gameLaunchService.WaitForGameToExitAsync();
                        await Dispatcher.UIThread.InvokeAsync(() =>
                        {
                            Status = "Game closed";
                            AddLogMessage("Game process exited");
                        });
                    });
                }
            }
            else
            {
                Status = "Failed to launch game";
                AddLogMessage("Failed to launch game");
            }
        }
        catch (Exception ex)
        {
            Status = $"Launch failed: {ex.Message}";
            AddLogMessage($"Launch error: {ex.Message}");
        }
        finally
        {
            if (!_gameLaunchService.IsGameRunning())
            {
                SetOperationState(false);
            }
        }
    }

    private async Task RepairGameAsync()
    {
        try
        {
            SetOperationState(true);
            _cancellationTokenSource = new CancellationTokenSource();

            // First verify integrity
            var isValid = await _updateService.VerifyGameIntegrityAsync(_cancellationTokenSource.Token);
            
            if (isValid)
            {
                Status = "Game integrity check passed";
                AddLogMessage("No repair needed - game files are valid");
            }
            else
            {
                Status = "Repairing game files...";
                var success = await _updateService.RepairGameAsync(_cancellationTokenSource.Token);
                
                if (success)
                {
                    Status = "Game repair completed";
                    AddLogMessage("Game repaired successfully");
                }
                else
                {
                    Status = "Game repair failed";
                    AddLogMessage("Game repair failed");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Status = "Repair cancelled";
            AddLogMessage("Repair cancelled by user");
        }
        catch (Exception ex)
        {
            Status = $"Repair failed: {ex.Message}";
            AddLogMessage($"Repair error: {ex.Message}");
        }
        finally
        {
            SetOperationState(false);
        }
    }

    private void CancelCurrentOperation()
    {
        _cancellationTokenSource?.Cancel();
        AddLogMessage("Operation cancelled by user");
    }

    private void OpenSettings()
    {
        // TODO: Implement settings window
        AddLogMessage("Settings window not implemented yet");
    }

    private void SetOperationState(bool isOperating, string? statusOverride = null)
    {
        IsUpdating = isOperating;
        CanPlay = !isOperating;
        CanRepair = !isOperating;
        
        if (statusOverride != null)
        {
            Status = statusOverride;
        }

        if (!isOperating)
        {
            ProgressValue = 0;
            ProgressText = "";
            IsProgressIndeterminate = false;
        }
    }

    private void OnProgressChanged(object? sender, UpdateProgress progress)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            ProgressValue = progress.OverallProgress;
            ProgressText = $"{progress.FilesCompleted}/{progress.TotalFiles} files";
            IsProgressIndeterminate = progress.IsIndeterminate;
            
            if (!string.IsNullOrEmpty(progress.CurrentFile))
            {
                Status = $"Processing: {progress.CurrentFile}";
            }
        });
    }

    private void OnStatusChanged(object? sender, string status)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            Status = status;
            AddLogMessage(status);
        });
    }

    private void OnErrorOccurred(object? sender, Exception ex)
    {
        Dispatcher.UIThread.InvokeAsync(() =>
        {
            AddLogMessage($"Error: {ex.Message}");
        });
    }

    private void AddLogMessage(string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        LogMessages.Add($"[{timestamp}] {message}");
        
        // Keep log size manageable
        while (LogMessages.Count > 100)
        {
            LogMessages.RemoveAt(0);
        }
    }
}