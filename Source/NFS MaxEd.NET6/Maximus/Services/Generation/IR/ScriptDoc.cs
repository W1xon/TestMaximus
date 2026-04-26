namespace Maximus.Services.IR;

public class ScriptDoc
{
    private readonly List<ScriptInstrucion> _instructions;
    public IReadOnlyList<ScriptInstrucion> Instructions => _instructions;
    
    public ScriptDoc()
    {
        _instructions = new List<ScriptInstrucion>();
    }
    public void AddInstruction(InstrucionType type, InstructionSection section = InstructionSection.NodeCreation, string? path = null, string? subject = null, string? subField = null, string? value = null)
    {
        var instruction = new ScriptInstrucion(type, section)
        {
            Path = path,
            Subject = subject,
            SubField = subField,
            Value = value
        };
         _instructions.Add(instruction);
    }
}