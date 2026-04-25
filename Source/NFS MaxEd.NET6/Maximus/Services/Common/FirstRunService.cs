using System.IO;

namespace Maximus.Services;

public static class FirstRunService
{
        private static readonly string SettingsPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "Maximus",
            "settings.txt"
        );

        public static bool IsFirstRun()
        {
            return !File.Exists(SettingsPath);
        }

        public static void MarkAsRun()
        {
            try
            {
                string directory = Path.GetDirectoryName(SettingsPath);
                if (!Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                File.WriteAllText(SettingsPath, DateTime.Now.ToString());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error marking first run: {ex.Message}");
            }
        }
    
}