using System.ComponentModel;

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
    [Description("========== Main Circuit Node ==========")]
    NodeCreation,

    [Description("---------- Field Declarations ----------")]
    FieldDeclaration,

    [Description("---------- Array Field Updates ----------")]
    ArrayUpdate,

    [Description("---------- Scalar Field Updates ----------")]
    ScalarUpdate,

    [Description("---------- Array Resizing ----------")]
    ArrayResize,

    [Description("========== Child Nodes ==========")]
    ChildNode,

    [Description("========== Children Entries ==========")]
    ChildEntrie,

    [Description("---------- Parent Container Updates ----------")]
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
    
    public ScriptInstrucion(InstrucionType type, InstructionSection section = InstructionSection.NodeCreation)
    {
        Type = type;
        Section = section;
    }
}