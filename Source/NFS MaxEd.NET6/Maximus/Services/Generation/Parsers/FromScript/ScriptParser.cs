using System.Globalization;
using Maximus.Converters;
using Maximus.Models;
using Maximus.Services.IR;
using Maximus.ViewModels;
using Barrier = Maximus.Models.Barrier;

namespace Maximus.Services.Parsers;

public class ScriptParser
{
    public void Parse(RaceConfig config, string script)
    {    
        var parser = new ScriptInstructionParser();
        var doc = parser.Parse(script);
        
        config.Reset();
        var addNodes = doc.Instructions.Where(i => i.Type == InstructionType.AddNode).ToList();
        SetRaceType(config, addNodes[0]);
        MainViewModel.Instance.UpdateVisibility();
        config.GameplayVault = addNodes[0].Path.Split('/')[1];
        config.RaceBin = addNodes[0].Path.Split('/')[0];
        
        foreach (var instrucion in doc.Instructions)
        {
            ParseScalarField(config, instrucion);
        }
        
        var updateFields = doc.Instructions.Where(i => i.Type == InstructionType.UpdateField).ToList();
        ParseStartAndFinish(config, updateFields);
        
        var barrierFields = updateFields.Where(i => i.Subject.Contains("Barriers")).ToList();
        foreach (var field in barrierFields)
            ParseBarrier(config, field);
        
        var checkpointFields = updateFields.Where(i => i.Path.Contains("checkpoint")).ToList();
        foreach(var field in checkpointFields)
            ParseCheckpoint(config, field);
        
        var shortcutFields = updateFields.Where(i => i.Path.Contains("shortcut")).ToList();
        foreach(var field in shortcutFields)
            ParseShortcut(config, field);
        
        var wrongwayFields = updateFields.Where(i => i.Path.Contains("wrongway")).ToList();
        foreach(var field in wrongwayFields)
            ParseWrongway(config, field);
        
        var timeBonusCheckpointFields = updateFields.Where(i => i.Path.Contains("time_bonus_checkpoint")).ToList();
        foreach(var field in timeBonusCheckpointFields)
            ParseTimeBonusCheckpoint(config, field);
        
        var trafficSpawnTriggerFields = updateFields.Where(i => i.Path.Contains("trafficspawntrigger") && !i.Path.Contains("character")).ToList();
        foreach(var field in trafficSpawnTriggerFields)
            ParseTrafficSpawnTrigger(config, field);
        
        var trafficSpawnCharacterFields = updateFields.Where(i => i.Path.Contains("trafficspawntrigger") && i.Path.Contains("character")).ToList();
        foreach(var field in trafficSpawnCharacterFields)
            ParseTrafficSpawnCharacter(config, field);
        
        var startMarkerFields = updateFields.Where(i => i.Path.Contains("start_marker")).ToList();
        var finishMarkerFields = updateFields.Where(i => i.Path.Contains("finish_marker")).ToList();
        foreach(var field in startMarkerFields)
            ParsePoint(config.StartMarker, field);
        foreach(var field in finishMarkerFields)
            ParsePoint(config.FinishMarker, field);
        
        var moneybagFields = updateFields.Where(i => i.Path.Contains("moneybag")).ToList();
        foreach (var field in moneybagFields)
        {
            if(field.Path.Contains("moneybag_small"))
                ParseMoneybag(config, field, "moneybag_small");
            else if(field.Path.Contains("moneybag_middle"))
                ParseMoneybag(config, field, "moneybag_middle");
            else if(field.Path.Contains("moneybag_big"))
                ParseMoneybag(config, field, "moneybag_big");
        }
        
        var speedtrapFields = updateFields.Where((i => i.Path.Contains("speedtrap"))).ToList();
        foreach(var field in speedtrapFields)
            ParseSpeedtrap(config, field);
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
    private void SetRaceType(RaceConfig config, ScriptInstruction field)
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
    private void ParseStartAndFinish(RaceConfig config, List<ScriptInstruction> updateFields)
    {
        var startGridFields = updateFields.Where(i => i.Path.Contains("startgrid")).ToList();
        var finishLineFields = updateFields.Where(i => i.Path.Contains("finishline")).ToList();
        foreach (var t in startGridFields)
            ParsePoint(config.StartGrid, t);

        foreach (var t in finishLineFields)
        {
            ParsePoint(config.FinishLine, t);
            
            if (t.Subject == "Dimensions")
            {
                if(t.SubField == "X")
                    config.FinishLine.DimensionsX = ParseFloat(t.Value);
                else if(t.SubField == "Y")
                    config.FinishLine.DimensionsY = ParseFloat(t.Value);
                else if(t.SubField == "Z")
                    config.FinishLine.DimensionsZ = ParseFloat(t.Value);
            }
        }
    }

    private void ParseSpeedtrap(RaceConfig config, ScriptInstruction field)
    {
        string name = field.Path.Split("/").Last();
        int index = ParseInt(name.Substring("speedtrap".Length));
        SpeedtrapEntity speedtrapEntity;
        if (index > config.Speedtraps.Count)
        {
            speedtrapEntity = new SpeedtrapEntity();
            speedtrapEntity.Name = name;
            config.Speedtraps.Add(speedtrapEntity);
        }
        if(index == config.Speedtraps.Count)
        {
            speedtrapEntity = config.Speedtraps[index - 1];
            ParsePoint(speedtrapEntity.Point, field);
        }
    }

    private void ParseMoneybag(RaceConfig config, ScriptInstruction field, string namePrefix)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring(namePrefix.Length));
        EntityType entityType = namePrefix switch
        {
            "moneybag_small" => EntityType.moneybag_small,
            "moneybag_middle" => EntityType.moneybag_middle,
            "moneybag_big" => EntityType.moneybag_big,
            _ => throw new FormatException($"Invalid moneybag type with prefix {namePrefix}")
        };
        List<Moneybag> moneybags = config.Moneybags.Where(m => m.SelectedType == entityType).ToList();
        Moneybag? moneybag;
        if(index > moneybags.Count)
        {
            moneybag = new Moneybag(entityType, name);
            config.Moneybags.Add(moneybag);
            moneybags.Add(moneybag);
        }
        if(index == moneybags.Count)
        {
            moneybag = moneybags[index - 1];
            ParsePoint(moneybag.Point, field);
        }
    }
    private void ParseTrafficSpawnCharacter(RaceConfig config, ScriptInstruction field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("character".Length));
        CharacterDrug? characterDrug = null;
        if(index > config.CharacterDrugs.Count)
        {
            characterDrug = new CharacterDrug();
            characterDrug.Name = name;
            config.CharacterDrugs.Add(characterDrug);
        }
        if(index == config.CharacterDrugs.Count)
            characterDrug = config.CharacterDrugs[index - 1];

        if (characterDrug == null) return;
        
        if(field.Subject == "CarType")
            characterDrug.SelectedCarType = field.Value;
    }
    private void ParseTrafficSpawnTrigger(RaceConfig config, ScriptInstruction field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("trafficspawntrigger".Length));
        TrafficSpawnTrigger? trafficSpawnTrigger = null;
        if(index > config.TrafficSpawnTriggers.Count)
        {
            trafficSpawnTrigger = new TrafficSpawnTrigger();
            trafficSpawnTrigger.Name = name;
            config.TrafficSpawnTriggers.Add(trafficSpawnTrigger);
        }
        if(index == config.TrafficSpawnTriggers.Count)
        {
            trafficSpawnTrigger = config.TrafficSpawnTriggers[index - 1];
            ParsePoint(trafficSpawnTrigger.Point, field);
        }

        if (trafficSpawnTrigger == null) return;
        
        if(field.Subject == "InitialSpeed" && config.InitialSpeed == 0)
            config.InitialSpeed = ParseInt(field.Value);
        if(field.Subject == "Radius")
            trafficSpawnTrigger.Radius = ParseFloat(field.Value);
    }
    private void ParseTimeBonusCheckpoint(RaceConfig config, ScriptInstruction field)
    {
        List<CheckpointEntity> timeBonusCheckpoints = config.Checkpoints.Where(c => c.EntityType == EntityType.timebonuscheckpoint).ToList();
        string name = field.Path.Split('/').Last().Split("_").Last();
        int index = ParseInt(name.Substring("checkpoint".Length));
        CheckpointEntity? timeBonusCheckpoint = null;
        if(index > timeBonusCheckpoints.Count)
        {
            timeBonusCheckpoint = new CheckpointEntity(EntityType.timebonuscheckpoint, $"/{name}");
            config.Checkpoints.Add(timeBonusCheckpoint);
            timeBonusCheckpoints.Add(timeBonusCheckpoint);
        }
        if (index == timeBonusCheckpoints.Count)
        {
            timeBonusCheckpoint = timeBonusCheckpoints[index - 1];
            ParsePoint(timeBonusCheckpoint.Point, field);
        }

        if (timeBonusCheckpoint is null) return;
            if(field.Subject == "TimeBonus")
                timeBonusCheckpoint.TimeBonus = ParseInt(field.Value);
    }
    
    private void ParseWrongway(RaceConfig config, ScriptInstruction field)
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
            ParsePoint(wrongway, field);
        }
    }
    private void ParseShortcut(RaceConfig config, ScriptInstruction field)
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
            ParsePoint(shortcut.Point, field);
        }

        if (shortcut is null) return;
        
        if(field.Subject == "ShortcutMaxChance")
            shortcut.MaxChance = ParseFloat(field.Value);
        else if(field.Subject == "ShortcutMinChance")
            shortcut.MinChance = ParseFloat(field.Value);
    }
    private void ParseCheckpoint(RaceConfig config, ScriptInstruction field)
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
            ParsePoint(checkpoint.Point, field);
        }
    }
    private void ParseBarrier(RaceConfig config, ScriptInstruction field)
    {
        config.Barriers.Add(new Barrier(field.Value.Replace("BARRIER_SPLINE_","")));
    }
    private void ParsePoint(PointEntity point, ScriptInstruction field)
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
    private void ParseScalarField(RaceConfig config, ScriptInstruction i)
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
            case"postraceactivity":
                config.PostRaceActivity = i.Value;
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
        if (float.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result))
            return result;

        return 0f;
    }
}