using System.Net.Http;
using System.Text.Json;
using Maximus.Views;

namespace Maximus.Services;


public class UpdateChecker
{
    public static string? LastCheckError { get; private set; }
    public static bool HasChecked;
    public static bool IsUpdateAvailable;
    private readonly string _currentVersion;
    private readonly string _updateUrl;
    private readonly TimeSpan _timeout;

    private UpdateChecker(TimeSpan? timeout = null)
    {
        _currentVersion = App.CurrentVersion;
        _updateUrl = App.UpdateUrl;
        _timeout = timeout ?? TimeSpan.FromSeconds(10);
    }

    public static async Task CheckForUpdatesAsync()
    {
        LastCheckError = null;
        var checker = new UpdateChecker();
        var updateInfo = await checker.GetRemoteVersionInfo();

        if (updateInfo != null)
        {
            ShowUpdateWindow(updateInfo);
            IsUpdateAvailable = true;
        }
        HasChecked = true;
    }

    private static void ShowUpdateWindow(RemoteVersionInfo info)
    {
        var window = new UpdateWindow(info);

        if (info.Mandatory)
        {
            window.ShowDialog();
        }
        else
        {
            window.Show();
        }
    }
    private async Task<RemoteVersionInfo?> GetRemoteVersionInfo()
    {
        try
        {
            using var client = new HttpClient { Timeout = _timeout };
           
            string json = await client.GetStringAsync(_updateUrl);
          
            var remoteInfo = JsonSerializer.Deserialize<RemoteVersionInfo>(
                json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (remoteInfo == null)
            {
                SetLastError("Update check failed: version.json is empty or invalid.");
                return null;
            }

            if (!remoteInfo.TryValidate(out var validationError))
            {
                SetLastError("Update check failed: " + validationError);
                return null;
            }

            // Критическое обновление (версия ниже минимально поддерживаемой)
            if (IsLowerVersion(_currentVersion, remoteInfo.MinSupportedVersion))
            {
                remoteInfo.Mandatory = true;
                return remoteInfo;
            }

            // Обычное обновление (доступна новая версия)
            if (IsHigherVersion(remoteInfo.Version, _currentVersion))
            {
                return remoteInfo;
            }

            return null;
        }
        catch (HttpRequestException ex)
        {
            SetLastError("Update check failed: network error. " + ex.Message);
            return null;
        }
        catch (TaskCanceledException ex)
        {
            SetLastError("Update check failed: request timeout. " + ex.Message);
            return null;
        }
        catch (JsonException ex)
        {
            SetLastError("Update check failed: invalid JSON. " + ex.Message);
            return null;
        }
        catch (Exception ex)
        {
            SetLastError("Update check failed: unexpected error. " + ex.Message);
            return null;
        }
    }

    private static void SetLastError(string message)
    {
        LastCheckError = message;
    }
    private bool IsLowerVersion(string current, string target)
    {
        try
        {
            return new Version(current) < new Version(target);
        }
        catch
        {
            return false;
        }
    }
    private bool IsHigherVersion(string remote, string local)
    {
        try
        {
            return new Version(remote) > new Version(local);
        }
        catch
        {
            return false;
        }
    }
}
