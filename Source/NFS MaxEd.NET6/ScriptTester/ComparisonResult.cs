namespace ScriptTester;
public record ComparisonRow(int LineNumber, string GoldenLine, string TestLine, bool IsMatch);

public class ComparisonResult
{
    public List<ComparisonRow> Rows { get; } = new();
    public bool IsFullMatch { get; set; } = true;
    public string Summary { get; set; } = "";
}