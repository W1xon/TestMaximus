using Maximus.Models;
using Maximus.Services.IR;
using Maximus.Services.IR.Renderers;

namespace Maximus.Services;

public partial class RaceScriptBuilder
{
    private readonly RaceType _nodeType;
    public RaceScriptBuilder(string path, RaceType nodeType)
    {
        Renderer = new RaceRenderer();
        _path = path;
        _nodeType = nodeType;
        AddMainNode(nodeType);
    }
    public RaceScriptBuilder AddField(string fieldName, object? value = null)
    {
        Doc.AddInstruction(
            InstrucionType.AddField,
            InstructionSection.FieldDeclaration,
            _path,
            fieldName,
            value: value?.ToString());
        
        /*_fieldDeclarations.Add(
            value is null
                ? $"add_field gameplay {_path} {fieldName}"
                : $"add_field gameplay {_path} {fieldName} {value}"
        );*/
        return this;
    }
    public RaceScriptBuilder UpdateField<T>(string fieldName, T value)
    {
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            InstructionSection.ScalarUpdate,
            _path,
            fieldName,
            value: value?.ToString());
        //_scalarUpdates.Add($"update_field gameplay {_path} {fieldName} {value}");
        return this;
    }
    public RaceScriptBuilder UpdateArrayItem(string arrayName, int index, string valuePath)
    {
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            InstructionSection.ArrayUpdate,
            _path,
            $"{arrayName}[{index}]",
            value: valuePath);
        //_arrayUpdates.Add($"update_field gameplay {_path} {arrayName}[{index}] {valuePath}");
        return this;
    }
    public RaceScriptBuilder UpdateArrayItemRelative(string arrayName, int index, string target)
    {
        var childPath = $"{_path}/{target}";
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            InstructionSection.ArrayUpdate,
            _path,
            $"{arrayName}[{index}]",
            value: childPath);
       //_arrayUpdates.Add($"update_field gameplay {_path} {arrayName}[{index}] {childPath}");
        return this;
    }

    public RaceScriptBuilder ResizeArray(string arrayName, int count)
    {
        Doc.AddInstruction(
            InstrucionType.ResizeField,
            InstructionSection.ArrayResize,
            _path,
            arrayName,
            value: count.ToString());
        //_arrayResizes.Add($"resize_field gameplay {_path} {arrayName} {count}");
        return this;
    }
    public RaceScriptBuilder AddFieldChild(string childName, string fieldName, object? value = null)
    {
        var childPath = $"{_path}/{childName}";
        Doc.AddInstruction(
            InstrucionType.AddField,
            InstructionSection.ChildNode,
            childPath,
            fieldName,
            value: value?.ToString()
            );
        /*_childNodes.Add(value is not null
            ? $"add_field gameplay {childPath} {fieldName} {value}"
            : $"add_field gameplay {childPath} {fieldName}");*/
        return this;
    }
    public RaceScriptBuilder UpdateFieldChildValue(string childName, string fieldName, object value)
    {
        var childPath = $"{_path}/{childName}";
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            InstructionSection.ChildNode,
            childPath,
            fieldName,
            value: value.ToString());
        //_childNodes.Add($"update_field gameplay {childPath} {fieldName} {value}");
        return this;
    }
    public RaceScriptBuilder UpdateNestedFieldChild(string childName, string fieldName, string subField, object value)
    {
        var childPath = $"{_path}/{childName}";
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            InstructionSection.ChildNode,
            childPath,
            fieldName,
            subField,
            value: value.ToString());
        //_childNodes.Add($"update_field gameplay {childPath} {fieldName} {subField} {value}");
        return this;
    }
    public RaceScriptBuilder UpdateArrayItemChild(string childName, string arrayName, int index, string target)
    {
        var childPath = $"{_path}/{childName}";
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            InstructionSection.ArrayUpdate,
            childPath,
            $"{arrayName}[{index}]",
            value: $"{childPath}/{target}");
        //_arrayUpdates.Add($"update_field gameplay {childPath} {arrayName}[{index}] {childPath}/{target}");
        return this;
    }
    
    public RaceScriptBuilder UpdateFieldChildReference(string childName, string fieldName, string referenceName)
    {
        var childPath = $"{_path}/{childName}";
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            InstructionSection.ChildNode,
            childPath,
            fieldName,
            value: $"{childPath}/{referenceName}");
        //_childNodes.Add($"update_field gameplay {childPath} {fieldName} {childPath}/{referenceName}");
        return this;
    }
    
    private RaceScriptBuilder AddChildToChildren(string childPath, bool autoincrement = true)
    {
            Doc.AddInstruction(
                InstrucionType.UpdateField,
                InstructionSection.ChildEntrie,
                _path,
                $"Children[{_childIndex}]",
                value: childPath);
        //_childrenEntries.Add($"update_field gameplay {_path} Children[{_childIndex}] {childPath}");
        if(autoincrement)
            _childIndex++;
        return this;
    }
    
    public RaceScriptBuilder AddFieldCharacter(string trafficTriggerName, string childName, string fieldName, object? value = null)
    {
        var childPath = $"{_path}/{trafficTriggerName}/{childName}";
            Doc.AddInstruction(
                InstrucionType.AddField,
                InstructionSection.ChildNode,
                childPath,
                fieldName,
                value: value?.ToString());
            /*
        /*_childNodes.Add(value is not null
            ? $"add_field gameplay {childPath} {fieldName} {value}"
            : $"add_field gameplay {childPath} {fieldName}");*/
        return this;
    }
    
    public RaceScriptBuilder UpdateFieldCharacterValue(string trafficTriggerName, string childName, string fieldName, object value)
    {
        var childPath = $"{_path}/{trafficTriggerName}/{childName}";
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            InstructionSection.ChildNode,
            childPath,
            fieldName,
            value: value.ToString());
        //_childNodes.Add($"update_field gameplay {childPath} {fieldName} {value}");
        return this;
    }
    
    public RaceScriptBuilder UpdateFieldChildReferenceToParent(string childName, string fieldName, string referenceName)
    {
        var childPath = $"{_path}/{childName}";
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            InstructionSection.ChildNode,
            childPath,
            fieldName,
            value: $"{_path}/{referenceName}");
        //_childNodes.Add($"update_field gameplay {childPath} {fieldName} {_path}/{referenceName}");
        return this;
    }
    
    public RaceScriptBuilder UpdateArrayItemChildToParent(string childName, string arrayName, int index, string target)
    {
        var childPath = $"{_path}/{childName}";
        Doc.AddInstruction(
            InstrucionType.UpdateField,
            InstructionSection.ChildNode,
            childPath,
            $"{arrayName}[{index}]",
            value: $"{_path}/{target}");
        //_childNodes.Add($"update_field gameplay {childPath} {arrayName}[{index}] {_path}/{target}");
        return this;
    }
    
    public RaceScriptBuilder AddNodeComment(string nodeName, string? prefix = null)
    {
        var displayName = prefix != null ? $"{prefix}: {nodeName}" : nodeName;
        Doc.AddInstruction(
            InstrucionType.Comment,
            InstructionSection.ChildNode,
            value: displayName);
        //_childNodes.Add($"// ---------- {displayName.ToUpper()} ----------");
        return this;
    }
    public RaceScriptBuilder AddNode(string type, string nodePath)
    {
        Doc.AddInstruction(
            InstrucionType.AddNode,
            InstructionSection.ChildNode,
            nodePath,
            type);
        //_childNodes.Add($"add_node gameplay {type} {nodePath}");
        return this;
    }
    
    public RaceScriptBuilder ChangeVault(string nodePath, string vault)
    {
        Doc.AddInstruction(
            InstrucionType.ChangeVault,
            InstructionSection.ChildNode,
            nodePath,
            value: vault);
        //_childNodes.Add($"change_vault gameplay {nodePath} {vault}");
        return this;
    }
    
    public RaceScriptBuilder InitializeNode(string type, string nodePath, string vault, string? commentPrefix = null)
    {
        var nodeName = nodePath.Split('/').Last();
        AddNodeComment(nodeName, commentPrefix);
        AddNode(type, nodePath);
        ChangeVault(nodePath, vault);
        return this;
    }
}