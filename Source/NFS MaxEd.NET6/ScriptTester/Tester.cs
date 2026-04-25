using Microsoft.Win32;

namespace ScriptTester;

public class Tester
{
    private OpenFileDialog _openFileDialog;

    public Tester()
    {
        _openFileDialog = new OpenFileDialog();
    }

    public string Check(string goldenScript, string testScript)
    {
        if (string.IsNullOrWhiteSpace(goldenScript) || string.IsNullOrWhiteSpace(testScript))
            return "Ошибка ввода: оба скрипта должны быть непустыми.";

        string[] goldenLines = goldenScript.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string[] testLines = testScript.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        
        bool isMatchLength = goldenLines.Length == testLines.Length;
        
        for (int i = 0; i < Math.Min(goldenLines.Length, testLines.Length); i++)
        {
            if (!string.Equals(goldenLines[i].Trim(), testLines[i].Trim(), StringComparison.Ordinal))
            {
                string lengthMismatch = isMatchLength ? "" : $" (Количество строк не совпадает: ожидалось {goldenLines.Length}, получено {testLines.Length})";
                return $"{lengthMismatch}\nСодержимое отличается в строке {i + 1}:\nОжидалось: {goldenLines[i]}\nПолучено: {testLines[i]}";
            }
        }

        if (!isMatchLength)
        {
            return $"Содержимое совпадает частично, но количество строк разное: ожидалось {goldenLines.Length}, получено {testLines.Length}.";
        }

        return "Скрипты полностью совпадают.";
    }

    public string[] OpenFile()
    {
        if (_openFileDialog.ShowDialog() == true)
        {
            return _openFileDialog.FileNames;
        }
        return Array.Empty<string>();
    }
}