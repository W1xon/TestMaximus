using System.Windows;
using Updater.ViewModels;

namespace Updater;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _viewModel;
    private readonly CancellationTokenSource _cts = new();
    private DateTime _windowShownAtUtc;

    private static readonly TimeSpan MinimumVisibleDuration = TimeSpan.FromSeconds(2);
    private static readonly TimeSpan SuccessVisibleDuration = TimeSpan.FromSeconds(2);

    public MainWindow(MainWindowViewModel viewModel)
    {
        _viewModel = viewModel;
        DataContext = _viewModel;
        InitializeComponent();
        Loaded += OnLoaded;
        Closing += OnClosing;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        try
        {
            // Give WPF a chance to complete first render before heavy async workflow starts.
            await Dispatcher.InvokeAsync(() => { }, System.Windows.Threading.DispatcherPriority.Render);
            _windowShownAtUtc = DateTime.UtcNow;

            var ok = await _viewModel.StartUpdateAsync(_cts.Token);
            if (ok)
            {
                await EnsureMinimumVisibleDurationAsync();
                await Task.Delay(SuccessVisibleDuration, _cts.Token);
                Application.Current.Shutdown();
                return;
            }

            await EnsureMinimumVisibleDurationAsync();

            if (!string.IsNullOrWhiteSpace(_viewModel.LastError))
            {
                MessageBox.Show(_viewModel.LastError, "Ошибка обновления", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when window closes while async delays are pending.
        }
    }

    private async Task EnsureMinimumVisibleDurationAsync()
    {
        var elapsed = DateTime.UtcNow - _windowShownAtUtc;
        var remaining = MinimumVisibleDuration - elapsed;
        if (remaining > TimeSpan.Zero)
        {
            await Task.Delay(remaining, _cts.Token);
        }
    }

    private void OnClosing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (_viewModel.IsBusy)
        {
            e.Cancel = true;
            return;
        }

        _cts.Cancel();
        _cts.Dispose();
    }

    private void CloseButton_OnClick(object sender, RoutedEventArgs e)
    {
        Close();
    }
}