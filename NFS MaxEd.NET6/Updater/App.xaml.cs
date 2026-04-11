using System.Windows;
using Updater.Models;
using Updater.Services;
using Updater.ViewModels;

namespace Updater;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
	protected override void OnStartup(StartupEventArgs e)
	{
		base.OnStartup(e);

		if (!UpdaterLaunchOptions.TryParse(e.Args, out var options, out var error))
		{
			MessageBox.Show(error, "Maximus Updater", MessageBoxButton.OK, MessageBoxImage.Error);
			Shutdown();
			return;
		}

		var viewModel = new MainWindowViewModel(new UpdateService(), options);
		var window = new MainWindow(viewModel);
		MainWindow = window;
		window.Show();
	}
}