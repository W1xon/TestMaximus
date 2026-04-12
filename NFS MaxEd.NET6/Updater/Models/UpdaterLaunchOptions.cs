using System.IO;
using Shared;

namespace Updater.Models;

public sealed class UpdaterLaunchOptions
{
    public string ProcessName { get; init; } = string.Empty;
    public string UpdateZipPath { get; init; } = string.Empty;
    public string TargetDirectory { get; init; } = AppDomain.CurrentDomain.BaseDirectory;
    public string RestartExecutableName { get; init; } = "Maximus.exe";

    public static bool TryParse(string[] args, out UpdaterLaunchOptions options, out string error)
    {
        options = new UpdaterLaunchOptions();
        error = string.Empty;

        if (args.Length != 3)
        {
            error = "Некорректные параметры запуска. Ожидалось: <process> <target> <restart>.";
            return false;
        }

        var process = args[0];
        var targetDir = args[1];
        var restartExe = args[2];

        if (string.IsNullOrWhiteSpace(process) || string.IsNullOrWhiteSpace(targetDir) || string.IsNullOrWhiteSpace(restartExe))
        {
            error = "Параметры запуска не должны быть пустыми.";
            return false;
        }

        var normalizedProcessName = Path.GetFileNameWithoutExtension(process);
        var resolvedTargetDir = Path.GetFullPath(targetDir);
        var resolvedZipPath = UpdatePackageSettings.GetArchivePath(resolvedTargetDir);

        options = new UpdaterLaunchOptions
        {
            ProcessName = normalizedProcessName,
            UpdateZipPath = resolvedZipPath,
            TargetDirectory = resolvedTargetDir,
            RestartExecutableName = Path.GetFileName(restartExe)
        };

        return true;
    }
}

