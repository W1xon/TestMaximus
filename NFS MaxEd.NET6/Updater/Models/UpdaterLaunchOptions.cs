using System.IO;

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

        if (args.Length == 0)
        {
            error = "Параметры запуска не переданы. Ожидалось: --process <name> --zip <path>.";
            return false;
        }

        string? process = null;
        string? zipPath = null;
        string? targetDir = null;
        string? restartExe = null;

        if (args.Length >= 2 && !args[0].StartsWith("--", StringComparison.Ordinal))
        {
            process = args[0];
            zipPath = args[1];
            if (args.Length >= 3)
            {
                targetDir = args[2];
            }
        }
        else
        {
            for (int i = 0; i < args.Length; i++)
            {
                var key = args[i];
                if (i + 1 >= args.Length)
                {
                    break;
                }

                var value = args[i + 1];
                switch (key)
                {
                    case "--process":
                    case "-p":
                        process = value;
                        i++;
                        break;
                    case "--zip":
                    case "-z":
                        zipPath = value;
                        i++;
                        break;
                    case "--target":
                    case "-t":
                        targetDir = value;
                        i++;
                        break;
                    case "--restart":
                    case "-r":
                        restartExe = value;
                        i++;
                        break;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(process) || string.IsNullOrWhiteSpace(zipPath))
        {
            error = "Некорректные параметры запуска. Нужно передать имя процесса и путь к архиву обновления.";
            return false;
        }

        var normalizedProcessName = Path.GetFileNameWithoutExtension(process);
        var resolvedZipPath = Path.GetFullPath(zipPath);
        var resolvedTargetDir = string.IsNullOrWhiteSpace(targetDir)
            ? AppDomain.CurrentDomain.BaseDirectory
            : Path.GetFullPath(targetDir);

        options = new UpdaterLaunchOptions
        {
            ProcessName = normalizedProcessName,
            UpdateZipPath = resolvedZipPath,
            TargetDirectory = resolvedTargetDir,
            RestartExecutableName = string.IsNullOrWhiteSpace(restartExe)
                ? normalizedProcessName + ".exe"
                : Path.GetFileName(restartExe)
        };

        return true;
    }
}

