using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Updater.Models;

namespace Updater.Services;

public sealed class UpdateService
{
    private const string MainExecutableName = "Maximus.exe";

    public async Task RunUpdateAsync(UpdaterLaunchOptions options, IProgress<UpdateProgressInfo>? progress, CancellationToken cancellationToken)
    {
        if (Directory.Exists(options.UpdateZipPath))
        {
            throw new InvalidDataException("Update package path points to a directory, not a ZIP file.");
        }

        if (!File.Exists(options.UpdateZipPath))
        {
            throw new FileNotFoundException("Update package was not found.", options.UpdateZipPath);
        }

        Report(progress, "Waiting for Maximus to close...", 5, true);
        await WaitForProcessExitAsync(options.ProcessName, timeoutSeconds: 10, cancellationToken);

        Report(progress, "Preparing update...", 20, true);
        await Task.Delay(500, cancellationToken);

        var tempDir = Path.Combine(Path.GetTempPath(), "Maximus_Update_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(tempDir);

        try
        {
            Report(progress, "Extracting update package...", 30, true);
            await Task.Run(() => ZipFile.ExtractToDirectory(options.UpdateZipPath, tempDir, true), cancellationToken);

            var files = Directory.GetFiles(tempDir, "*", SearchOption.AllDirectories);
            var totalFiles = Math.Max(files.Length, 1);
            var copiedFiles = 0;

            foreach (var sourceFilePath in files)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var relativePath = Path.GetRelativePath(tempDir, sourceFilePath);
                var targetFilePath = Path.Combine(options.TargetDirectory, relativePath);
                var fileName = Path.GetFileName(sourceFilePath);
                var progressFileName = fileName;

                if (ShouldSkipUpdaterOwnedFile(relativePath))
                {
                    progressFileName += " (skipped)";
                }
                else
                {
                    var targetFolder = Path.GetDirectoryName(targetFilePath);
                    if (!string.IsNullOrWhiteSpace(targetFolder))
                    {
                        Directory.CreateDirectory(targetFolder);
                    }

                    var verifyAsMain = fileName.Equals(MainExecutableName, StringComparison.OrdinalIgnoreCase);
                    if (verifyAsMain && File.Exists(targetFilePath))
                    {
                        File.Delete(targetFilePath);
                    }

                    File.Copy(sourceFilePath, targetFilePath, true);
                }

                copiedFiles++;
                ReportCopyProgress(progress, copiedFiles, totalFiles, progressFileName);
            }

            Report(progress, "Cleaning temporary files...", 93, true);
            if (File.Exists(options.UpdateZipPath))
            {
                File.Delete(options.UpdateZipPath);
            }
        }
        finally
        {
            TryDeleteDirectory(tempDir);
        }

        var mainExePath = Path.Combine(options.TargetDirectory, options.RestartExecutableName);
        if (!File.Exists(mainExePath))
        {
            throw new FileNotFoundException("Maximus executable was not found after update.", mainExePath);
        }

        Report(progress, "Starting updated app...", 98, true);
        Process.Start(new ProcessStartInfo
        {
            FileName = mainExePath,
            WorkingDirectory = options.TargetDirectory,
            UseShellExecute = true
        });

        Report(progress, "Update completed", 100, false);
    }

    private static async Task WaitForProcessExitAsync(string processName, int timeoutSeconds, CancellationToken cancellationToken)
    {
        var normalizedName = Path.GetFileNameWithoutExtension(processName);
        var timeout = TimeSpan.FromSeconds(timeoutSeconds);

        foreach (var process in Process.GetProcessesByName(normalizedName))
        {
            try
            {
                if (process.HasExited)
                {
                    continue;
                }

                var waitTask = process.WaitForExitAsync(cancellationToken);
                var completedTask = await Task.WhenAny(waitTask, Task.Delay(timeout, cancellationToken));
                if (completedTask != waitTask && !process.HasExited)
                {
                    process.Kill();
                    process.WaitForExit(2000);
                }
            }
            finally
            {
                process.Dispose();
            }
        }
    }

    private static bool ShouldSkipUpdaterOwnedFile(string relativePath)
    {
        string fileName = Path.GetFileName(relativePath);

        if (fileName.StartsWith("Updater.", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        string normalizedRelativePath = relativePath.Replace('/', '\\');
        return normalizedRelativePath.StartsWith("Updater\\", StringComparison.OrdinalIgnoreCase);
    }


    private static void ReportCopyProgress(IProgress<UpdateProgressInfo>? progress, int copied, int total, string fileName)
    {
        var percent = 35 + (copied / (double)total) * 55;
        Report(progress, "Copying files: " + fileName, percent, false);
    }

    private static void Report(IProgress<UpdateProgressInfo>? progress, string message, double percent, bool isIndeterminate)
    {
        progress?.Report(new UpdateProgressInfo
        {
            Message = message,
            Percent = Math.Clamp(percent, 0, 100),
            IsIndeterminate = isIndeterminate
        });
    }

    private static void TryDeleteDirectory(string path)
    {
        try
        {
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }
        }
        catch
        {
        }
    }
}
