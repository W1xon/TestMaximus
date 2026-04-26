namespace Maximus.Services.IR;
public enum InstrucionType
{
    Comment,
    AddNode,
    AddField,
    UpdateField,
    ChangeVault,
    ResizeField,
}

public enum InstructionSection
{
    NodeCreation,
    FieldDeclaration,
    ArrayUpdate,
    ScalarUpdate,
    ArrayResize,
    ChildNode,
    ChildEntrie,
    ParentUpdate
}
public class ScriptInstrucion
{
    public InstrucionType Type { get; }
    public InstructionSection Section { get; }
    public string Scope { get; } = "gameplay";
    public string? Path { get; set; }
    public string? Subject { get; set; }
    public string? SubField { get; set; }
    public string? Value { get; set; }
    
    public ScriptInstrucion(InstrucionType type, InstructionSection section)
    {
        Type = type;
        Section = section;
    }
}