using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Updater.Models;
using Updater.Services;

namespace Updater.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly UpdateService _updateService;
    private readonly UpdaterLaunchOptions _options;

    private bool _hasStarted;
    private bool _isBusy;
    private bool _isIndeterminate = true;
    private double _progressValue;
    private string _statusText = "Подготовка...";
    private string? _lastError;

    public MainWindowViewModel(UpdateService updateService, UpdaterLaunchOptions options)
    {
        _updateService = updateService;
        _options = options;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<string> LogItems { get; } = new();

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (_isBusy == value)
            {
                return;
            }

            _isBusy = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(CanClose));
        }
    }

    public bool CanClose => !IsBusy;

    public bool IsIndeterminate
    {
        get => _isIndeterminate;
        private set
        {
            if (_isIndeterminate == value)
            {
                return;
            }

            _isIndeterminate = value;
            OnPropertyChanged();
        }
    }

    public double ProgressValue
    {
        get => _progressValue;
        private set
        {
            if (Math.Abs(_progressValue - value) < 0.001)
            {
                return;
            }

            _progressValue = value;
            OnPropertyChanged();
        }
    }

    public string StatusText
    {
        get => _statusText;
        private set
        {
            if (_statusText == value)
            {
                return;
            }

            _statusText = value;
            OnPropertyChanged();
        }
    }

    public string? LastError
    {
        get => _lastError;
        private set
        {
            if (_lastError == value)
            {
                return;
            }

            _lastError = value;
            OnPropertyChanged();
        }
    }

    public async Task<bool> StartUpdateAsync(CancellationToken cancellationToken)
    {
        if (_hasStarted)
        {
            return false;
        }

        _hasStarted = true;
        IsBusy = true;
        LastError = null;

        var progress = new Progress<UpdateProgressInfo>(OnProgressReported);

        try
        {
            await _updateService.RunUpdateAsync(_options, progress, cancellationToken);
            StatusText = "Обновление установлено. Завершение...";
            IsIndeterminate = false;
            ProgressValue = 100;
            return true;
        }
        catch (Exception ex)
        {
            LastError = ex.Message;
            StatusText = "Ошибка обновления";
            AddLog("Ошибка: " + ex.Message);
            return false;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void OnProgressReported(UpdateProgressInfo info)
    {
        StatusText = info.Message;
        IsIndeterminate = info.IsIndeterminate;
        if (!info.IsIndeterminate)
        {
            ProgressValue = info.Percent;
        }

        AddLog(info.Message);
    }

    private void AddLog(string message)
    {
        if (LogItems.Count == 0 || LogItems[^1] != message)
        {
            LogItems.Add(message);
        }
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

