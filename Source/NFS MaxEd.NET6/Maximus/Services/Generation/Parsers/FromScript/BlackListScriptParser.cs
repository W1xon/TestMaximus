using Maximus.Models;
using Maximus.Services.IR;

namespace Maximus.Services.Parsers;

public class BlackListScriptParser
{
    private BlacklistConfig _config;
    public BlackListScriptParser(BlacklistConfig config)
    {
        _config = config;
    }
    public void Parse(string script)
    {
        var parser = new ScriptInstructionParser();
        var doc = parser.Parse(script);
        
        _config.Reset();
        var updateInstructions = doc.Instructions.Where(i => i.Type == InstructionType.UpdateField);
        foreach (var field in updateInstructions)
            ParseScalarField(field);
        

        _config.RaceBin = updateInstructions.FirstOrDefault().Path;
        int index = 0, firstSq = 0, secondSq = 0;
        char strFirstSq = '[', strSecondSq = ']';
        var worldRaceInstructions = doc.Instructions.Where(i => i.Subject.Contains("WorldRaces["));
        if(worldRaceInstructions.Count() > 0)
        {
            foreach (var instruction in worldRaceInstructions)
            {
                firstSq = instruction.Subject.IndexOf(strFirstSq);
                secondSq = instruction.Subject.IndexOf(strSecondSq);
                index = ParseInt(instruction.Subject.Substring(firstSq + 1, secondSq - firstSq - 1));
                if(_config.WorldRaces.Count < index + 1)
                    _config.WorldRaces.Add(instruction.Value.Split("/").LastOrDefault());
                else
                    _config.WorldRaces[index] = instruction.Value.Split("/").LastOrDefault();   
            }
            if(_config.WorldRaces.Count > index + 1)
                for(int i = index+1; i < _config.WorldRaces.Count;)
                    _config.WorldRaces.RemoveAt(i);
        }
        
        var childrenInstructions =
            doc.Instructions.Where(i => i.Subject.Contains("Children[") && !i.Path.Contains("milestones"));
        if(childrenInstructions.Count() > 0)
        {
            foreach (var instruction in childrenInstructions)
            {
                firstSq = instruction.Subject.IndexOf(strFirstSq);
                secondSq = instruction.Subject.IndexOf(strSecondSq);
                index = ParseInt(instruction.Subject.Substring(firstSq + 1, secondSq - firstSq - 1));
                if(_config.Children.Count < index + 1)
                    _config.Children.Add(instruction.Value.Split("/").LastOrDefault());
                else
                    _config.Children[index] = instruction.Value.Split("/").LastOrDefault();
            }
            if(_config.Children.Count > index + 1)
                for(int i = index+1; i < _config.Children.Count;)
                    _config.Children.RemoveAt(i);
        }
        
        var milestoneInstructions =
            doc.Instructions.Where(i => i.Subject.Contains("Children[") && i.Path.Contains("milestones"));
        if(milestoneInstructions.Count() > 0)
        {
            foreach (var instruction in milestoneInstructions)
            {
                firstSq = instruction.Subject.IndexOf(strFirstSq);
                secondSq = instruction.Subject.IndexOf(strSecondSq);
                index = ParseInt(instruction.Subject.Substring(firstSq + 1, secondSq - firstSq - 1));
                if(_config.Milestones.Count < index + 1)
                    _config.Milestones.Add(instruction.Value);
                else
                    _config.Milestones[index] = instruction.Value;
            }
            if(_config.Milestones.Count > index + 1)
                for(int i = index+1; i < _config.Milestones.Count; )
                    _config.Milestones.RemoveAt(i);
        }
        
        var bossRaceInstructions = doc.Instructions.Where(i => i.Subject.Contains("BossRaces["));
        if (bossRaceInstructions.Count() > 0)
        {
            foreach (var instruction in bossRaceInstructions)
            {
                firstSq = instruction.Subject.IndexOf(strFirstSq);
                secondSq = instruction.Subject.IndexOf(strSecondSq);
                index = ParseInt(instruction.Subject.Substring(firstSq + 1, secondSq - firstSq - 1));
                if(_config.BossRaces.Count < index + 1)
                    _config.BossRaces.Add(instruction.Value.Split("/").LastOrDefault());
                else
                    _config.BossRaces[index] = instruction.Value.Split("/").LastOrDefault();
            }
            if(_config.BossRaces.Count > index + 1)
                for(int i = index+1; i < _config.BossRaces.Count;)
                    _config.BossRaces.RemoveAt(i);
        }
    }


    private void ParseScalarField(ScriptInstruction instruction)
    {
        if (instruction.Value is null) return;
        
        switch (instruction.Subject)
        {
            case "RequiredBounty":
                _config.RequiredBounty = ParseInt(instruction.Value);
                break;
            case "RequiredChallenges":
                _config.RequiredChallenges = ParseInt(instruction.Value);
                break;
            case "RequiredRacesWon":
                _config.RequiredRaceWon = ParseInt(instruction.Value);
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