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

    protected override string HandleInstruction(ScriptInstruction i)
    {
        return i.Type switch
        {
            InstructionType.Comment => $"// ---------- {i.Value.ToUpper()} ----------",
            InstructionType.ResizeField => $"resize_field {i.Scope} {i.Path} {i.Subject} {i.Value}",
            InstructionType.UpdateField => $"update_field {i.Scope} {i.Path} {i.Subject} {i.Value}"
        };
    }
}