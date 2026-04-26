namespace Maximus.Services.IR.Renderers;

public class MilestonesRenderer : BaseRenderer
{
    private readonly List<InstrucionType> _call;
    public MilestonesRenderer()
    {
        _call =  new ()
        {
            InstrucionType.AddField,
            InstrucionType.UpdateField
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

    protected override string HandleInstruction(ScriptInstrucion i)
    {
        return i.Type switch
        {
            InstrucionType.Comment => $"// ---------- {i.Value.ToUpper()} ----------",
            InstrucionType.AddNode => $"add_node {i.Scope} {i.Subject} {i.Path}",
            InstrucionType.AddField => $"add_field {i.Scope} {i.Path} {i.Subject}",
            InstrucionType.UpdateField => $"update_field {i.Scope} {i.Path} {i.Subject} {i.Value}"
        };
    }
}