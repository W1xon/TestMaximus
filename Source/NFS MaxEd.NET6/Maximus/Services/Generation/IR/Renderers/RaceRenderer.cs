using System.Text;

namespace Maximus.Services.IR.Renderers;

public class RaceRenderer : BaseRenderer
{
    private readonly List<InstructionSection> _call;
    public RaceRenderer()
    {
        _call =  new ()
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
    }
    public override string Render(ScriptDoc doc)
    {
        var groupByCall = doc.Instructions.GroupBy(i => i.Section)
            .OrderBy(g => _call.IndexOf(g.Key));

        foreach (var group in groupByCall)
        {
            foreach (var instruction in group)
            {
                Sb.AppendLine(HandleInstruction(instruction));
            }
        }

        return Sb.ToString();
    }

    protected override string HandleInstruction(ScriptInstrucion i)
    {
        return i.Type switch
        {
            InstrucionType.Comment => $"// ---------- {i.Value.ToUpper()} ----------",
            InstrucionType.AddNode => $"add_node {i.Scope} {i.Subject} {i.Path}",
            InstrucionType.AddField => i.Value is null 
                ? $"add_field {i.Scope} {i.Path} {i.Subject}"
                : $"add_field {i.Scope} {i.Path} {i.Subject} {i.Value}",
            InstrucionType.UpdateField => i.SubField is null
                ? $"update_field {i.Scope} {i.Path} {i.Subject} {i.Value}"
                : $"update_field {i.Scope} {i.Path} {i.Subject} {i.SubField} {i.Value}",
            InstrucionType.ResizeField => $"resize_field {i.Scope} {i.Path} {i.Subject} {i.Value}",
            InstrucionType.ChangeVault => $"change_vault {i.Scope} {i.Path} {i.Subject}",
        };
    }
}