using System.IO;
using Microsoft.Win32;
using Maximus.ViewModels;

namespace Maximus.Services;

public class FileService
{
    public static void SaveNFSMS(string code, string articleName = "")
    {
        SaveFileDialog saveFileDialog = new SaveFileDialog
        {
            Filter = "NFS Files (*.nfsms)|*.nfsms",
            FileName = $"Maximus_{articleName}.nfsms",
            Title = "Сохранить код"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            File.WriteAllText(saveFileDialog.FileName, code);
        }
    }
}