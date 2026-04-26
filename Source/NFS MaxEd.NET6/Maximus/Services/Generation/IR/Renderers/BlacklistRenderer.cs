namespace Maximus.Services.IR.Renderers;

public class BlacklistRenderer : BaseRenderer
{
    
    public BlacklistRenderer()
    {
    }
    public override string Render(ScriptDoc doc)
    {
        
        foreach (var instruction in doc.Instructions)
        {
            Sb.AppendLine(HandleInstruction(instruction));
        }

        return Sb.ToString();
    }

    protected override string HandleInstruction(ScriptInstrucion i)
    {
        return i.Type switch
        {
            InstrucionType.Comment => $"// ---------- {i.Value.ToUpper()} ----------",
            InstrucionType.ResizeField => $"resize_field {i.Scope} {i.Path} {i.Subject} {i.Value}",
            InstrucionType.UpdateField => $"update_field {i.Scope} {i.Path} {i.Subject} {i.Value}"
        };
    }
}