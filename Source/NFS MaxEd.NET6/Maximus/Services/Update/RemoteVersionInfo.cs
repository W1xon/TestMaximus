namespace Maximus.Services;

public class RemoteVersionInfo
{
    public string Version { get; set; } = string.Empty;
    public bool Mandatory { get; set; }
    public string MinSupportedVersion { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public List<string> Changelog { get; set; } = new();
    public string DownloadUrl { get; set; } = string.Empty;

    public bool TryValidate(out string error)
    {
        if (string.IsNullOrWhiteSpace(Version))
        {
            error = "Missing required field: Version.";
            return false;
        }

        if (!global::System.Version.TryParse(Version, out _))
        {
            error = $"Invalid field Version: '{Version}'.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(MinSupportedVersion))
        {
            error = "Missing required field: MinSupportedVersion.";
            return false;
        }

        if (!global::System.Version.TryParse(MinSupportedVersion, out _))
        {
            error = $"Invalid field MinSupportedVersion: '{MinSupportedVersion}'.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(DownloadUrl))
        {
            error = "Missing required field: DownloadUrl.";
            return false;
        }

        if (!Uri.TryCreate(DownloadUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
        {
            error = $"Invalid field DownloadUrl: '{DownloadUrl}'.";
            return false;
        }

        error = string.Empty;
        return true;
    }
}
