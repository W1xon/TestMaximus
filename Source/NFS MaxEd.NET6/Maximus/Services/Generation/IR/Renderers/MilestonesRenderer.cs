namespace Maximus.Services.IR.Renderers;

public class MilestonesRenderer : BaseRenderer
{
    private readonly List<InstructionType> _call;
    public MilestonesRenderer()
    {
        _call =  new ()
        {
            InstructionType.AddField,
            InstructionType.UpdateField
        };
    }
    public override string Render(ScriptDoc doc)
    {
        var groupByCall = doc.Instructions.GroupBy(i => i.Type)
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

    protected override string HandleInstruction(ScriptInstruction i)
    {
        return i.Type switch
        {
            InstructionType.Comment => $"// ---------- {i.Value.ToUpper()} ----------",
            InstructionType.AddNode => $"add_node {i.Scope} {i.Subject} {i.Path}",
            InstructionType.AddField => $"add_field {i.Scope} {i.Path} {i.Subject}",
            InstructionType.UpdateField => $"update_field {i.Scope} {i.Path} {i.Subject} {i.Value}"
        };
    }
}