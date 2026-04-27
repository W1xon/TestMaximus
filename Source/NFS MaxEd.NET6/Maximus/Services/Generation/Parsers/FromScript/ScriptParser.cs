using Maximus.Converters;
using Maximus.Models;
using Maximus.Services.IR;
using Barrier = Maximus.Models.Barrier;

namespace Maximus.Services.Parsers;

public class ScriptParser
{
    public void Parse(RaceConfig config, string script)
    {    
        config.Reset();
        var parser = new ScriptInstructionParser();
        var doc = parser.Parse(script);
        foreach (var instrucion in doc.Instructions)
        {
            HandleScalarField(config, instrucion);
        }
        var addFields = doc.Instructions.Where(i => i.Type == InstrucionType.AddField).ToList();
        var addNodes = doc.Instructions.Where(i => i.Type == InstrucionType.AddNode).ToList();

        SetRaceType(config, addNodes[0]);
        config.GameplayVault = addNodes[0].Path.Split('/')[1];
        config.RaceBin = addNodes[0].Path.Split('/')[0];
        var updateFields = doc.Instructions.Where(i => i.Type == InstrucionType.UpdateField).ToList();

        HandleStartAndFinish(config, updateFields);
        var barrierFields = updateFields.Where(i => i.Subject.Contains("Barriers")).ToList();
        foreach (var field in barrierFields)
            HandleBarrier(config, field);
        
        var checkpointFields = updateFields.Where(i => i.Path.Contains("checkpoint")).ToList();
        foreach(var field in checkpointFields)
            HandleCheckpoint(config, field);
        
        var shortcutFields = updateFields.Where(i => i.Path.Contains("shortcut")).ToList();
        foreach(var field in shortcutFields)
            HandleShortcut(config, field);
        
        var wrongwayFields = updateFields.Where(i => i.Path.Contains("wrongway")).ToList();
        foreach(var field in wrongwayFields)
            HandleWrongway(config, field);
        
        var timeBonusCheckpointFields = updateFields.Where(i => i.Path.Contains("time_bonus_checkpoint")).ToList();
        foreach(var field in timeBonusCheckpointFields)
            HandleTimeBonusCheckpoint(config, field);

        SortCheckpoints(config);
    }
    
    private void SortCheckpoints(RaceConfig config)
    {
        var ordered = config.Checkpoints
            .OrderBy(c => ExtractIndex(c.Name))
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            var correctItem = ordered[i];
            var currentIndex = config.Checkpoints.IndexOf(correctItem);

            if (currentIndex != i)
                config.Checkpoints.Move(currentIndex, i);
        }
    }
    private int ExtractIndex(string name)
    {
        var digits = new string(name.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var value) ? value : int.MaxValue;
    }
    private void SetRaceType(RaceConfig config, ScriptInstrucion field)
    {
        config.NodeType = ParseRaceType(field.Subject);

        switch (config.NodeType)
        {
            case RaceType.lapknockout:
            case RaceType.circuit:
                config.IsCircuitOrKnockout = true;
                break;
            case RaceType.tollboothrace:
                config.IsTollbooth = true;
                break;
            case RaceType.speedtraprace:
                config.IsSpeedtrap = true;
                break;
            case RaceType.cashgrab:
                config.IsCashgrab = true;
                break;
            case RaceType.drag:
                config.IsDrag = true;
                break;
        }
    }
    
    private void HandleStartAndFinish(RaceConfig config, List<ScriptInstrucion> updateFields)
    {
        var startGridFields = updateFields.Where(i => i.Path.Contains("startgrid")).ToList();
        var finishLineFields = updateFields.Where(i => i.Path.Contains("finishline")).ToList();
        for(int i  = 0; i < startGridFields.Count; i++)
        {
            HandlePoint(config.StartGrid, startGridFields[i]);
            HandlePoint(config.FinishLine, finishLineFields[i]);
        }
    }
    private void HandleTimeBonusCheckpoint(RaceConfig config, ScriptInstrucion field)
    {
        List<CheckpointEntity> timeBonusCheckpoints = config.Checkpoints.Where(c => c.EntityType == EntityType.timebonuscheckpoint).ToList();
        string name = field.Path.Split('/').Last().Split("_").Last();
        int index = ParseInt(name.Substring("checkpoint".Length));
        CheckpointEntity? timeBonusCheckpoint = null;
        if(index > timeBonusCheckpoints.Count)
        {
            timeBonusCheckpoint = new CheckpointEntity(EntityType.timebonuscheckpoint, $"/{name}");
            config.Checkpoints.Add(timeBonusCheckpoint);
        }
        if (index == timeBonusCheckpoints.Count)
        {
            timeBonusCheckpoint = timeBonusCheckpoints[index - 1];
            HandlePoint(timeBonusCheckpoint.Point, field);
        }

        if (timeBonusCheckpoint is null) return;
            if(field.Subject == "TimeBonus")
                timeBonusCheckpoint.TimeBonus = ParseInt(field.Value);
    }
    
    private void HandleWrongway(RaceConfig config, ScriptInstrucion field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("wrongway".Length));
        PointEntity wrongway;
        if(index > config.ResetPlayerTrigers.Count)
        {
            wrongway = new PointEntity(EntityType.resetplayertrigger, name);
            config.ResetPlayerTrigers.Add(wrongway);
        }
        if(index == config.ResetPlayerTrigers.Count)
        {
            wrongway = config.ResetPlayerTrigers[index - 1];
            HandlePoint(wrongway, field);
        }
    }
    private void HandleShortcut(RaceConfig config, ScriptInstrucion field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("shortcut".Length));
        ShortcutEntity? shortcut = null;
        if (index > config.Shortcuts.Count)
        {
            shortcut = new ShortcutEntity(name);
            config.Shortcuts.Add(shortcut);
        }

        if (index == config.Shortcuts.Count)
        {
            shortcut = config.Shortcuts[index - 1];
            HandlePoint(shortcut.Point, field);
        }

        if (shortcut is null) return;
        
        if(field.Subject == "ShortcutMaxChance")
            shortcut.MaxChance = ParseFloat(field.Value);
        else if(field.Subject == "ShortcutMinChance")
            shortcut.MinChance = ParseFloat(field.Value);
    }
    private void HandleCheckpoint(RaceConfig config, ScriptInstrucion field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("checkpoint".Length));
        CheckpointEntity checkpoint;
        if(index > config.Checkpoints.Count)
        {
            checkpoint = new CheckpointEntity(EntityType.checkpoint, name);
            config.Checkpoints.Add(checkpoint);
        }

        if (index == config.Checkpoints.Count)
        {
            checkpoint = config.Checkpoints[index - 1];
            HandlePoint(checkpoint.Point, field);
        }
    }
    private void HandleBarrier(RaceConfig config, ScriptInstrucion field)
    {
        config.Barriers.Add(new Barrier(field.Value.Replace("BARRIER_SPLINE_","")));
    }
    private void HandlePoint(PointEntity point, ScriptInstrucion field)
    {
        if (field.Subject == "Position")
        {
            float value = ParseFloat(field.Value);
            if (field.SubField == "X")
                point.PositionX = value;
            else if (field.SubField == "Y")
                point.PositionY = value;
            else if (field.SubField == "Z")
                point.PositionZ = value;
        }

        if (field.Subject == "Rotation")
        {
            point.RotationHEX = RotationConverter.DegreesToHex(ParseFloat(field.Value));
        }
    }
    private void HandleScalarField(RaceConfig config, ScriptInstrucion i)
    {
        if (i.Value is null) return;
        
        switch (i.Subject.ToLower())
        {
            // Флаги (bool)
            case "availableqr":
                config.AvailableQR = ParseBool(i.Value);
                break;
            case "bossrace":
            case "bossraces":
                config.BossRaces = ParseBool(i.Value);
                break;
            case "enablecops":
                config.EnableCops = ParseBool(i.Value);
                break;
            case "quickracenis":
                config.QuickRaceNis = ParseBool(i.Value);
                break;
            case "ischanceofrain":
                config.IsChanceOfRain = ParseBool(i.Value);
                break;
    
            // Строковые параметры и списки
            case "region":
                config.Region = i.Value;
                break;
            case "intronis":
                config.IntroNis = i.Value;
                break;
            case "finishcamera":
                config.FinishCamera = i.Value;
                break;
            case "gameplayvault":
                config.GameplayVault = i.Value;
                break;
            case "eventid":
                config.EventId = i.Value;
                break;
            case "template":
                config.Template = i.Value;
                break;
    
            // Числовые параметры (int)
            case "opponents": // В скрипте Opponents, в коде Opponent
                config.Opponent = ParseInt(i.Value);
                break;
            case "cashvalue":
                config.CashValue = ParseInt(i.Value);
                break;
            case "trafficlevel":
                config.TrafficLevel = ParseInt(i.Value);
                break;
            case "racelength":
                config.RaceLength = ParseInt(i.Value);
                break;
            case "numlaps":
                config.NumLaps = ParseInt(i.Value);
                break;
            case "chanceofrain":
                config.ChanceOfRain = ParseInt(i.Value);
                break;
            case "timelimit":
                config.TimeLimit = ParseInt(i.Value);
                break;
            case "reputation":
                config.Reputation = ParseInt(i.Value);
                break;
            case "raceLength":
                config.RaceLength = ParseInt(i.Value);
                break;
    
            // Числовые (double/float)
            case "rivalbesttime":
                config.RivalBestTime = ParseFloat(i.Value);
                break;
        }
    }

    private RaceType ParseRaceType(string value)
    {
        return value.ToLower() switch
        {
            "circuit" => RaceType.circuit,
            "p2p" => RaceType.p2p,
            "lapknockout" => RaceType.lapknockout,
            "tollboothrace" => RaceType.tollboothrace,
            "speedtraprace" => RaceType.speedtraprace,
            "cashgrab" => RaceType.cashgrab,
            "drag" => RaceType.drag,
            _ => throw new FormatException($"Value {value} is not a valid RaceType")
        };
    }

    private bool ParseBool(string value)
    {
        return value.ToLower() switch
        {
            "true" => true,
            "false" => false,
            _ => throw new FormatException($"Value {value} is not a valid boolean")
        };
    }
    private int ParseInt(string value)
    {
        if (int.TryParse(value, out var result))
            return result;
        return 0;
    }
    private float ParseFloat(string value)
    {
        if (float.TryParse(value, out var result))
            return result;
        return 0;
    }
}