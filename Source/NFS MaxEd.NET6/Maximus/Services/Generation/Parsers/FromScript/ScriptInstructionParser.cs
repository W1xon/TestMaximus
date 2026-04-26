using Maximus.Services.IR;

namespace Maximus.Services.Parsers;

public class ScriptInstructionParser
{
    private ScriptDoc _doc;
    public ScriptDoc Parse(string script)
    {
        _doc = new ScriptDoc();
        foreach (var line in script.Split('\n'))
        {
            if(string.IsNullOrWhiteSpace(line) || line.Contains("//"))
                continue;
            HandleLine(line.Trim());
        }
        return _doc;
    }

    private void HandleLine(string line)
    {
        string[] parts = line.Split(' ');
        switch (parts[0])
        {
            case "add_node":
                HandleAddNode(line);
                break;
            case "add_field":
                HandleAddField(line);
                break;
            case "update_field":
                HandleUpdateField(line);
                break;
            case "change_vault":
                HandleChangeVault(line);
                break;
            case "resize_field":
                HandleResizeField(line);
                break;
            default:
                throw new NotImplementedException($"Instruction {parts[0]} not implemented");
        }
    }

    private void HandleResizeField(string line)
    {
        var parts = line.Split(' ');
        // resize_field gameplay <path> <field> <new_size>
        var path = parts[2];
        var field = parts[3];
        var newSize = parts[4];
        _doc.AddInstruction(
            InstrucionType.ResizeField,
            path: path,
            subject: field,
            value: newSize);
    }

    private void HandleChangeVault(string line)
    {
        var parts = line.Split(' ');    
        // change_vault gameplay <path> <vault>
        var path = parts[2];
        var vault = parts[3];
        _doc.AddInstruction(
            InstrucionType.ChangeVault,
            path: path,
            subject: vault);
    }

    private void HandleUpdateField(string line)
    {
        var parts = line.Split(' ');    
        // update_field gameplay <path> <field> [subfield] <value>
        var path = parts[2];
        var field = parts[3];
        var subField = parts.Length > 5 ? parts[4] : null;
        var value = parts.Length > 5 ? parts[5] : parts[4];
        _doc.AddInstruction(
            InstrucionType.UpdateField,
            path: path,
            subject: field,
            subField: subField,
            value: value);
    }

    private void HandleAddField(string line)
    {
        var parts = line.Split(' ');    
        // add_field gameplay <path> <field> [value]
        var path = parts[2];
        var field = parts[3];
        var value = parts.Length > 4 ? parts[4] : null;
        _doc.AddInstruction(
            InstrucionType.AddField,
            path: path,
            subject: field,
            value: value);
    }

    private void HandleAddNode(string line)
    {
        var parts = line.Split(' ');    
        // add_node gameplay <type> <path>
        var type = parts[2];
        var path = parts[3];
        _doc.AddInstruction(
            InstrucionType.AddNode,
            subject: type,
            path: path);
    }
}