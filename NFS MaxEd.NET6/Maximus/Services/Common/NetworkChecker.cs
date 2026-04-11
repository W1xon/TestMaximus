using System.Net.Http;
using System.Net.NetworkInformation;

namespace Maximus.Services;

public static class NetworkChecker
{
    private static readonly HttpClient _client = new()
    {
        Timeout = TimeSpan.FromSeconds(5)
    };

    public static async Task<bool> IsInternetAvailableAsync( string probeUrl = "http://www.gstatic.com/generate_204")
    {
        
        if (!NetworkInterface.GetIsNetworkAvailable()) return false;

        try
        {
            using var response = await _client.GetAsync(probeUrl, HttpCompletionOption.ResponseHeadersRead);
            return response.IsSuccessStatusCode;
        }
        catch { return false; }
    }

    public static bool IsNetworkError(Exception ex)
    {
        if (ex is HttpRequestException)
            return true;

        if (ex is TaskCanceledException)
            return true;

        if (ex is TimeoutException)
            return true;
        
        return false;
    }
}