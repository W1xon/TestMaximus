using Maximus.Models;
using Maximus.Services.IR;

namespace Maximus.Services.Parsers;

public class MilestoneScriptParser
{
    private MilestoneConfig _config;
    public MilestoneScriptParser(MilestoneConfig config)
    {
        _config = config;
    }
    public void Parse(string script)
    {
        
        var parser = new ScriptInstructionParser();
        var doc = parser.Parse(script);
        _config.Reset();
        
        var updateFields = doc.Instructions.Where(i => i.Type == InstructionType.UpdateField);
        foreach (var field in updateFields)
            ParseScalarField(field);
        
        var addNodeField = doc.Instructions.FirstOrDefault(i => i.Type == InstructionType.AddNode);
        int index = addNodeField.Path.Split('/').Last().IndexOf("milestone", StringComparison.OrdinalIgnoreCase);
        _config.MilestoneType =  addNodeField.Path.Split('/').Last().Substring(index);
    }
    
    private void ParseScalarField(ScriptInstruction instruction)
    {
       if (instruction.Value is null) return;
        
        switch (instruction.Subject)
        {
            case "BinIndex":
                _config.SelectedBinIndex = _config.BinIndices[ParseInt(instruction.Value) - 1] ;
                break;
            case "GoalEasy":
                _config.GoalEasy = ParseInt(instruction.Value);
                break;
            case "GoalHard":
                _config.GoalHard = ParseInt(instruction.Value);
                break;
            case "GoalAddPrevBest":
                _config.GoalAddPrevBest = ParseInt(instruction.Value);
                break;
            case "SpawnPoint":
                _config.SelectedSpawnPoint = instruction.Value;
                break;
        }
    }
    
    private int ParseInt(string value)
    {
        if (int.TryParse(value, out var result))
            return result;
        return 0;
    }
}