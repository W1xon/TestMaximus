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
        var ok = await _viewModel.StartUpdateAsync(_cts.Token);
        if (ok)
        {
            Application.Current.Shutdown();
            return;
        }

        if (!string.IsNullOrWhiteSpace(_viewModel.LastError))
        {
            MessageBox.Show(_viewModel.LastError, "Ошибка обновления", MessageBoxButton.OK, MessageBoxImage.Error);
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