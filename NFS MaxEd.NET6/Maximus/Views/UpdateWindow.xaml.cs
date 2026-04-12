using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Windows;
using Maximus.Services;
using Shared;

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
                ModernDialog.DialogType.Confirm);

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
        using var client = new HttpClient { Timeout = TimeSpan.FromMinutes(5) };
        string downloadUrl = ResolveArchiveDownloadUrl(_info.DownloadUrl);

        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string updateZipPath = UpdatePackageSettings.GetArchivePath(baseDir);
        string tempZipPath = updateZipPath + ".download";

        try
        {
            using (var response = await client.GetAsync(downloadUrl, HttpCompletionOption.ResponseHeadersRead))
            {
                response.EnsureSuccessStatusCode();

                await using var sourceStream = await response.Content.ReadAsStreamAsync();
                await using var targetStream = new FileStream(tempZipPath, FileMode.Create, FileAccess.Write, FileShare.None);
                await sourceStream.CopyToAsync(targetStream);
                await targetStream.FlushAsync();
            }

            if (File.Exists(updateZipPath))
            {
                File.Delete(updateZipPath);
            }

            File.Move(tempZipPath, updateZipPath);
        }
        finally
        {
            if (File.Exists(tempZipPath))
            {
                File.Delete(tempZipPath);
            }
        }

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
            WorkingDirectory = baseDir,
            UseShellExecute = true,
            Verb = "runas"
        };

        startInfo.ArgumentList.Add(mainProcessName);
        startInfo.ArgumentList.Add(baseDir);
        startInfo.ArgumentList.Add(mainProcessName + ".exe");

        var updaterProcess = Process.Start(startInfo);
        if (updaterProcess == null)
        {
            throw new InvalidOperationException("Updater process did not start.");
        }

        Application.Current.Shutdown();
    }


    private static string ResolveArchiveDownloadUrl(string rawUrl)
    {
        if (!Uri.TryCreate(rawUrl, UriKind.Absolute, out var uri))
        {
            return rawUrl;
        }

        // If metadata points to GitHub latest page, convert it to direct asset URL.
        if (uri.Host.Contains("github.com", StringComparison.OrdinalIgnoreCase) &&
            uri.AbsolutePath.EndsWith("/releases/latest", StringComparison.OrdinalIgnoreCase))
        {
            string directPath = uri.AbsolutePath.TrimEnd('/') + "/download/" + UpdatePackageSettings.ArchiveFileName;
            var builder = new UriBuilder(uri)
            {
                Path = directPath,
                Query = string.Empty,
                Fragment = string.Empty
            };
            return builder.Uri.ToString();
        }

        return rawUrl;
    }
}