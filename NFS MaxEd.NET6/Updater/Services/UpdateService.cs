using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using Updater.Models;

namespace Updater.Services;

public sealed class UpdateService : IUpdateService
{
    private const string UpdaterExecutableName = "Updater.exe";
    private const string MainExecutableName = "Maximus.exe";

    public async Task RunUpdateAsync(UpdaterLaunchOptions options, IProgress<UpdateProgressInfo>? progress, CancellationToken cancellationToken)
    {
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

                if (fileName.Equals(UpdaterExecutableName, StringComparison.OrdinalIgnoreCase))
                {
                    copiedFiles++;
                    ReportCopyProgress(progress, copiedFiles, totalFiles, fileName + " (skipped)");
                    continue;
                }

                var targetFolder = Path.GetDirectoryName(targetFilePath);
                if (!string.IsNullOrWhiteSpace(targetFolder))
                {
                    Directory.CreateDirectory(targetFolder);
                }

                var verifyAsMain = fileName.Equals(MainExecutableName, StringComparison.OrdinalIgnoreCase);
                await CopyFileWithRetryAsync(sourceFilePath, targetFilePath, verifyAsMain, cancellationToken);

                copiedFiles++;
                ReportCopyProgress(progress, copiedFiles, totalFiles, fileName);
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

        await Task.Run(() =>
        {
            var processes = Process.GetProcessesByName(normalizedName);
            foreach (var process in processes)
            {
                try
                {
                    var elapsed = 0;
                    while (!process.HasExited && elapsed < timeoutSeconds * 1000)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        Thread.Sleep(500);
                        elapsed += 500;
                    }

                    if (!process.HasExited)
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
        }, cancellationToken);
    }

    private static async Task CopyFileWithRetryAsync(string sourcePath, string targetPath, bool verifyMainFile, CancellationToken cancellationToken)
    {
        const int maxRetries = 5;

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                if (verifyMainFile && File.Exists(targetPath))
                {
                    File.Delete(targetPath);
                }

                await Task.Run(() => File.Copy(sourcePath, targetPath, true), cancellationToken);

                if (verifyMainFile)
                {
                    var sourceSize = new FileInfo(sourcePath).Length;
                    var targetSize = new FileInfo(targetPath).Length;
                    if (sourceSize != targetSize)
                    {
                        throw new IOException($"File size mismatch: {sourceSize} vs {targetSize}");
                    }
                }

                return;
            }
            catch when (attempt < maxRetries)
            {
                await Task.Delay(500, cancellationToken);
            }
        }

        throw new IOException($"Failed to copy file: {Path.GetFileName(sourcePath)}");
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
            // Ignore temp cleanup issues.
        }
    }
}
