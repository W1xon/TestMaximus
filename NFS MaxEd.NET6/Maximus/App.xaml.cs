using System.Reflection;
using System.Windows;
using Maximus.Services;

namespace Maximus;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    
    public static string CurrentVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";
    public const string UpdateUrl = "https://raw.githubusercontent.com/W1xon/.../main/version.json";
    protected override async void OnStartup(StartupEventArgs e)
    {
        if(await NetworkChecker.IsInternetAvailableAsync())
            await UpdateChecker.CheckForUpdatesAsync();
    }
}