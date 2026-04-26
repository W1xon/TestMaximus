using System.Windows.Media;
using Microsoft.Win32;

namespace ScriptTester;

public class Tester
{
    public ComparisonResult Check(string goldenScript, string testScript)
    {
        var result = new ComparisonResult();
        var gLines = goldenScript.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);
        var tLines = testScript.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None);

        int maxLines = Math.Max(gLines.Length, tLines.Length);

        for (int i = 0; i < maxLines; i++)
        {
            string gLine = i < gLines.Length ? gLines[i] : null;
            string tLine = i < tLines.Length ? tLines[i] : null;

            bool match = string.Equals(gLine?.Trim(), tLine?.Trim(), StringComparison.Ordinal);
            
            if (!match) result.IsFullMatch = false;

            result.Rows.Add(new ComparisonRow(i + 1, gLine ?? "[MISSING]", tLine ?? "[MISSING]", match));
        }

        result.Summary = result.IsFullMatch ? "Scripts match!" : "Differences found.";
        return result;
    }
}
public class ScriptPair
{
    public string FileName { get; set; }
    public string GoldenPath { get; set; }
    public string TestPath { get; set; }
    public bool? IsMatch { get; set; } // null - не проверено, true/false - результат
    
    public string StatusIcon => IsMatch == true ? "✔" : "❌";
    public Brush StatusColor => IsMatch == true ? Brushes.LightGreen : Brushes.Tomato;
}