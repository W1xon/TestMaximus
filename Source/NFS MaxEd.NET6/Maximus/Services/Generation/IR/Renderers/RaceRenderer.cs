using System.Text;

namespace Maximus.Services.IR.Renderers;

public class RaceRenderer
{
    private readonly List<InstructionSection> _call = new ()
    {
        InstructionSection.NodeCreation,
        InstructionSection.FieldDeclaration,
        InstructionSection.ArrayUpdate,
        InstructionSection.ArrayResize,
        InstructionSection.ScalarUpdate,
        InstructionSection.ChildEntrie,
        InstructionSection.ChildNode,
        InstructionSection.ParentUpdate,
    };
    private readonly StringBuilder _sb = new();
    public string Render(ScriptDoc doc)
    {
        var groupByCall = doc.Instructions.GroupBy(i => i.Section)
            .OrderBy(g => _call.IndexOf(g.Key));

        foreach (var group in groupByCall)
        {
            foreach (var instruction in group)
            {
                _sb.AppendLine(HandleInstruction(instruction));
            }
        }

        return _sb.ToString();
    }

    private string HandleInstruction(ScriptInstrucion i)
    {
        return i.Type switch
        {
            InstrucionType.Comment => $"// ---------- {i.Value.ToUpper()} ----------",
            InstrucionType.AddNode => $"add_node {i.Scope} {i.Subject} {i.Path}",
            InstrucionType.AddField => i.Value is null 
                ? $"add_field {i.Scope} {i.Path} {i.Subject}"
                : $"add_field {i.Scope} {i.Path} {i.Subject} {i.Value}",
            InstrucionType.UpdateField => $"update_field {i.Scope} {i.Path} {i.Subject} {i.Value}",
            InstrucionType.ResizeField => $"resize_field {i.Scope} {i.Path} {i.Subject} {i.Value}",
        };
    }
}