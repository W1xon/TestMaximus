using System.IO;

namespace Shared;

public static class UpdatePackageSettings
{
    public const string ArchiveFileName = "Maximus.zip";

    public static string GetArchivePath(string baseDirectory)
    {
        return Path.Combine(baseDirectory, ArchiveFileName);
    }
}

