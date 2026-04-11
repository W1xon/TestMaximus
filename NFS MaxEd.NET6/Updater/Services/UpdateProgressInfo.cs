namespace Updater.Services;

public sealed class UpdateProgressInfo
{
    public string Message { get; init; } = string.Empty;
    public double Percent { get; init; }
    public bool IsIndeterminate { get; init; }
}

