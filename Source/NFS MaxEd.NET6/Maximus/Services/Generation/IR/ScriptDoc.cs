namespace Maximus.Services.IR;

public class ScriptDoc
{
    private readonly List<ScriptInstruction> _instructions;
    public IReadOnlyList<ScriptInstruction> Instructions => _instructions;
    
    public ScriptDoc()
    {
        _instructions = new List<ScriptInstruction>();
    }
    public void AddInstruction(InstructionType type, InstructionSection section = InstructionSection.NodeCreation, string? path = null, string? subject = null, string? subField = null, string? value = null)
    {
        var instruction = new ScriptInstruction(type, section)
        {
            Path = path,
            Subject = subject,
            SubField = subField,
            Value = value
        };
         _instructions.Add(instruction);
    }
}