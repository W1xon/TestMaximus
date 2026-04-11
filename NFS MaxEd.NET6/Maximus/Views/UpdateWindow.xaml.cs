using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using Maximus.Services;

namespace Maximus.Views;

public partial class UpdateWindow : Window
{
    private readonly RemoteVersionInfo _info;

    public UpdateWindow(RemoteVersionInfo info)
    {
        InitializeComponent();
        _info = info;

        InitializeUI();
    }

    private void InitializeUI()
    {
        VersionText.Text = $"Версия {_info.Version}";
        DateText.Text = $"Выпуск: {_info.ReleaseDate:dd.MM.yyyy}";

        ChangelogList.ItemsSource = _info.Changelog;

        if (_info.Mandatory)
        {
            MandatoryWarning.Visibility = Visibility.Visible;
            SkipButton.Visibility = Visibility.Collapsed;
        }
        else
        {
            MandatoryWarning.Visibility = Visibility.Collapsed;
            SkipButton.Visibility = Visibility.Visible;
        }
    }

    private async void UpdateButton_Click(object sender, RoutedEventArgs e)
    {
        UpdateButton.IsEnabled = false;
        SkipButton.IsEnabled = false;
        
        ProgressContainer.Visibility = Visibility.Visible;
        ProgressText.Text = "Загрузка обновления...";

        try
        {
            await DownloadAndUpdateAsync();
        }
        catch (Exception ex)
        {
            ProgressContainer.Visibility = Visibility.Collapsed;
            
            ModernDialog.Show(
                $"Не удалось загрузить обновление: {ex.Message}",
                "Ошибка обновления",
                ModernDialog.DialogType.Error);

            UpdateButton.IsEnabled = true;
            SkipButton.IsEnabled = true;
        }
    }

    private void SkipButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void CloseBtn_Click(object sender, RoutedEventArgs e)
    {
        if (_info.Mandatory)
        {
            var result = ModernDialog.Show(
                "Это обязательное обновление. Приложение не может работать без него.\n\nЗакрыть приложение?",
                "Обязательное обновление",
                ModernDialog.DialogType.Warning);

            if (result == true)
            {
                Application.Current.Shutdown();
            }
        }
        else
        {
            Close();
        }
    }

    private async Task DownloadAndUpdateAsync()
    {
        using var client = new HttpClient
        {
            Timeout = TimeSpan.FromMinutes(5)
        };

        byte[] data = await client.GetByteArrayAsync(_info.DownloadUrl);

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string updateZipPath = Path.Combine(baseDir, "Maximus_Update.zip");

        await File.WriteAllBytesAsync(updateZipPath, data);

        string updaterPath = Path.Combine(baseDir, "Updater.exe");

        if (!File.Exists(updaterPath))
        {
            throw new FileNotFoundException(
                "Файл updater не найден. Переустановите приложение.",
                updaterPath);
        }

        string mainProcessName = Path.GetFileNameWithoutExtension(Process.GetCurrentProcess().MainModule?.FileName)
            ?? "Maximus";

        var startInfo = new ProcessStartInfo
        {
            FileName = updaterPath,
            Arguments = $"--process \"{mainProcessName}\" --zip \"{updateZipPath}\" --target \"{baseDir}\" --restart \"{mainProcessName}.exe\"",
            UseShellExecute = true,
            Verb = "runas"
        };

        Process.Start(startInfo);

        Application.Current.Shutdown();
    }
}