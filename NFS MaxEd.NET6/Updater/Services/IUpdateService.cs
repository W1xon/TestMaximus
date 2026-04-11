using Updater.Models;

namespace Updater.Services;

public interface IUpdateService
{
    Task RunUpdateAsync(UpdaterLaunchOptions options, IProgress<UpdateProgressInfo>? progress, CancellationToken cancellationToken);
}

