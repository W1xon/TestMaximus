using System.Globalization;
using Maximus.Converters;
using Maximus.Models;
using Maximus.Services.IR;
using Maximus.ViewModels;
using Barrier = Maximus.Models.Barrier;

namespace Maximus.Services.Parsers;

public class RaceScriptParser
{
    private RaceConfig _config;
    public RaceScriptParser(RaceConfig config)
    {
        _config = config;
    }
    public void Parse(string script)
    {    
        var parser = new ScriptInstructionParser();
        var doc = parser.Parse(script);
        
        _config.Reset();
        var addNodes = doc.Instructions.Where(i => i.Type == InstructionType.AddNode).ToList();
        SetRaceType(_config, addNodes[0]);
        MainViewModel.Instance.UpdateVisibility();
        _config.GameplayVault = addNodes[0].Path.Split('/')[1];
        _config.RaceBin = addNodes[0].Path.Split('/')[0];
        
        foreach (var instrucion in doc.Instructions)
        {
            ParseScalarField(_config, instrucion);
        }
        
        var updateFields = doc.Instructions.Where(i => i.Type == InstructionType.UpdateField).ToList();
        ParseStartAndFinish(_config, updateFields);
        
        var barrierFields = updateFields.Where(i => i.Subject.Contains("Barriers")).ToList();
        foreach (var field in barrierFields)
            ParseBarrier(_config, field);
        
        var checkpointFields = updateFields.Where(i => i.Path.Contains("checkpoint")).ToList();
        foreach(var field in checkpointFields)
            ParseCheckpoint(_config, field);
        
        var shortcutFields = updateFields.Where(i => i.Path.Contains("shortcut")).ToList();
        foreach(var field in shortcutFields)
            ParseShortcut(_config, field);
        
        var wrongwayFields = updateFields.Where(i => i.Path.Contains("wrongway")).ToList();
        foreach(var field in wrongwayFields)
            ParseWrongway(_config, field);
        
        var timeBonusCheckpointFields = updateFields.Where(i => i.Path.Contains("time_bonus_checkpoint")).ToList();
        foreach(var field in timeBonusCheckpointFields)
            ParseTimeBonusCheckpoint(_config, field);
        
        var trafficSpawnTriggerFields = updateFields.Where(i => i.Path.Contains("trafficspawntrigger") && !i.Path.Contains("character")).ToList();
        foreach(var field in trafficSpawnTriggerFields)
            ParseTrafficSpawnTrigger(_config, field);
        
        var trafficSpawnCharacterFields = updateFields.Where(i => i.Path.Contains("trafficspawntrigger") && i.Path.Contains("character")).ToList();
        foreach(var field in trafficSpawnCharacterFields)
            ParseTrafficSpawnCharacter(_config, field);
        
        var startMarkerFields = updateFields.Where(i => i.Path.Contains("start_marker")).ToList();
        var finishMarkerFields = updateFields.Where(i => i.Path.Contains("finish_marker")).ToList();
        foreach(var field in startMarkerFields)
            ParsePoint(_config.StartMarker, field);
        foreach(var field in finishMarkerFields)
            ParsePoint(_config.FinishMarker, field);
        
        var moneybagFields = updateFields.Where(i => i.Path.Contains("moneybag")).ToList();
        foreach (var field in moneybagFields)
        {
            if(field.Path.Contains("moneybag_small"))
                ParseMoneybag(_config, field, "moneybag_small");
            else if(field.Path.Contains("moneybag_middle"))
                ParseMoneybag(_config, field, "moneybag_middle");
            else if(field.Path.Contains("moneybag_big"))
                ParseMoneybag(_config, field, "moneybag_big");
        }
        
        var speedtrapFields = updateFields.Where((i => i.Path.Contains("speedtrap"))).ToList();
        foreach(var field in speedtrapFields)
            ParseSpeedtrap(_config, field);
        SortCheckpoints(_config);
    }
    
    private void SortCheckpoints(RaceConfig _config)
    {
        var ordered = _config.Checkpoints
            .OrderBy(c => ExtractIndex(c.Name))
            .ToList();

        for (int i = 0; i < ordered.Count; i++)
        {
            var correctItem = ordered[i];
            var currentIndex = _config.Checkpoints.IndexOf(correctItem);

            if (currentIndex != i)
                _config.Checkpoints.Move(currentIndex, i);
        }
    }
    private int ExtractIndex(string name)
    {
        var digits = new string(name.Where(char.IsDigit).ToArray());
        return int.TryParse(digits, out var value) ? value : int.MaxValue;
    }
    private void SetRaceType(RaceConfig _config, ScriptInstruction field)
    {
        _config.NodeType = ParseRaceType(field.Subject);

        switch (_config.NodeType)
        {
            case RaceType.lapknockout:
            case RaceType.circuit:
                _config.IsCircuitOrKnockout = true;
                break;
            case RaceType.tollboothrace:
                _config.IsTollbooth = true;
                break;
            case RaceType.speedtraprace:
                _config.IsSpeedtrap = true;
                break;
            case RaceType.cashgrab:
                _config.IsCashgrab = true;
                break;
            case RaceType.drag:
                _config.IsDrag = true;
                break;
        }
    }
    private void ParseStartAndFinish(RaceConfig _config, List<ScriptInstruction> updateFields)
    {
        var startGridFields = updateFields.Where(i => i.Path.Contains("startgrid")).ToList();
        var finishLineFields = updateFields.Where(i => i.Path.Contains("finishline")).ToList();
        foreach (var t in startGridFields)
            ParsePoint(_config.StartGrid, t);

        foreach (var t in finishLineFields)
        {
            ParsePoint(_config.FinishLine, t);
            
            if (t.Subject == "Dimensions")
            {
                if(t.SubField == "X")
                    _config.FinishLine.DimensionsX = ParseFloat(t.Value);
                else if(t.SubField == "Y")
                    _config.FinishLine.DimensionsY = ParseFloat(t.Value);
                else if(t.SubField == "Z")
                    _config.FinishLine.DimensionsZ = ParseFloat(t.Value);
            }
        }
    }

    private void ParseSpeedtrap(RaceConfig _config, ScriptInstruction field)
    {
        string name = field.Path.Split("/").Last();
        int index = ParseInt(name.Substring("speedtrap".Length));
        SpeedtrapEntity speedtrapEntity;
        if (index > _config.Speedtraps.Count)
        {
            speedtrapEntity = new SpeedtrapEntity();
            speedtrapEntity.Name = name;
            _config.Speedtraps.Add(speedtrapEntity);
        }
        if(index == _config.Speedtraps.Count)
        {
            speedtrapEntity = _config.Speedtraps[index - 1];
            ParsePoint(speedtrapEntity.Point, field);
        }
    }

    private void ParseMoneybag(RaceConfig _config, ScriptInstruction field, string namePrefix)
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
        List<Moneybag> moneybags = _config.Moneybags.Where(m => m.SelectedType == entityType).ToList();
        Moneybag? moneybag;
        if(index > moneybags.Count)
        {
            moneybag = new Moneybag(entityType, name);
            _config.Moneybags.Add(moneybag);
            moneybags.Add(moneybag);
        }
        if(index == moneybags.Count)
        {
            moneybag = moneybags[index - 1];
            ParsePoint(moneybag.Point, field);
        }
    }
    private void ParseTrafficSpawnCharacter(RaceConfig _config, ScriptInstruction field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("character".Length));
        CharacterDrug? characterDrug = null;
        if(index > _config.CharacterDrugs.Count)
        {
            characterDrug = new CharacterDrug();
            characterDrug.Name = name;
            _config.CharacterDrugs.Add(characterDrug);
        }
        if(index == _config.CharacterDrugs.Count)
            characterDrug = _config.CharacterDrugs[index - 1];

        if (characterDrug == null) return;
        
        if(field.Subject == "CarType")
            characterDrug.SelectedCarType = field.Value;
    }
    private void ParseTrafficSpawnTrigger(RaceConfig _config, ScriptInstruction field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("trafficspawntrigger".Length));
        TrafficSpawnTrigger? trafficSpawnTrigger = null;
        if(index > _config.TrafficSpawnTriggers.Count)
        {
            trafficSpawnTrigger = new TrafficSpawnTrigger();
            trafficSpawnTrigger.Name = name;
            _config.TrafficSpawnTriggers.Add(trafficSpawnTrigger);
        }
        if(index == _config.TrafficSpawnTriggers.Count)
        {
            trafficSpawnTrigger = _config.TrafficSpawnTriggers[index - 1];
            ParsePoint(trafficSpawnTrigger.Point, field);
        }

        if (trafficSpawnTrigger == null) return;
        
        if(field.Subject == "InitialSpeed" && _config.InitialSpeed == 0)
            _config.InitialSpeed = ParseInt(field.Value);
        if(field.Subject == "Radius")
            trafficSpawnTrigger.Radius = ParseFloat(field.Value);
    }
    private void ParseTimeBonusCheckpoint(RaceConfig _config, ScriptInstruction field)
    {
        List<CheckpointEntity> timeBonusCheckpoints = _config.Checkpoints.Where(c => c.EntityType == EntityType.timebonuscheckpoint).ToList();
        string name = field.Path.Split('/').Last().Split("_").Last();
        int index = ParseInt(name.Substring("checkpoint".Length));
        CheckpointEntity? timeBonusCheckpoint = null;
        if(index > timeBonusCheckpoints.Count)
        {
            timeBonusCheckpoint = new CheckpointEntity(EntityType.timebonuscheckpoint, $"/{name}");
            _config.Checkpoints.Add(timeBonusCheckpoint);
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
    
    private void ParseWrongway(RaceConfig _config, ScriptInstruction field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("wrongway".Length));
        PointEntity wrongway;
        if(index > _config.ResetPlayerTrigers.Count)
        {
            wrongway = new PointEntity(EntityType.resetplayertrigger, name);
            _config.ResetPlayerTrigers.Add(wrongway);
        }
        if(index == _config.ResetPlayerTrigers.Count)
        {
            wrongway = _config.ResetPlayerTrigers[index - 1];
            ParsePoint(wrongway, field);
        }
    }
    private void ParseShortcut(RaceConfig _config, ScriptInstruction field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("shortcut".Length));
        ShortcutEntity? shortcut = null;
        if (index > _config.Shortcuts.Count)
        {
            shortcut = new ShortcutEntity(name);
            _config.Shortcuts.Add(shortcut);
        }

        if (index == _config.Shortcuts.Count)
        {
            shortcut = _config.Shortcuts[index - 1];
            ParsePoint(shortcut.Point, field);
        }

        if (shortcut is null) return;
        
        if(field.Subject == "ShortcutMaxChance")
            shortcut.MaxChance = ParseFloat(field.Value);
        else if(field.Subject == "ShortcutMinChance")
            shortcut.MinChance = ParseFloat(field.Value);
    }
    private void ParseCheckpoint(RaceConfig _config, ScriptInstruction field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("checkpoint".Length));
        CheckpointEntity checkpoint;
        if(index > _config.Checkpoints.Count)
        {
            checkpoint = new CheckpointEntity(EntityType.checkpoint, name);
            _config.Checkpoints.Add(checkpoint);
        }

        if (index == _config.Checkpoints.Count)
        {
            checkpoint = _config.Checkpoints[index - 1];
            ParsePoint(checkpoint.Point, field);
        }
    }
    private void ParseBarrier(RaceConfig _config, ScriptInstruction field)
    {
        _config.Barriers.Add(new Barrier(field.Value.Replace("BARRIER_SPLINE_","")));
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
    private void ParseScalarField(RaceConfig _config, ScriptInstruction i)
    {
        if (i.Value is null) return;
        
        switch (i.Subject.ToLower())
        {
            // Флаги (bool)
            case "availableqr":
                _config.AvailableQR = ParseBool(i.Value);
                break;
            case "bossrace":
            case "bossraces":
                _config.BossRaces = ParseBool(i.Value);
                break;
            case "enablecops":
                _config.EnableCops = ParseBool(i.Value);
                break;
            case "quickracenis":
                _config.QuickRaceNis = ParseBool(i.Value);
                break;
            case "ischanceofrain":
                _config.IsChanceOfRain = ParseBool(i.Value);
                break;
    
            // Строковые параметры и списки
            case "region":
                _config.Region = i.Value;
                break;
            case "intronis":
                _config.IntroNis = i.Value;
                break;
            case "finishcamera":
                _config.FinishCamera = i.Value;
                break;
            case "gameplayvault":
                _config.GameplayVault = i.Value;
                break;
            case "eventid":
                _config.EventId = i.Value;
                break;
            case "template":
                _config.Template = i.Value;
                break;
            case"postraceactivity":
                _config.PostRaceActivity = i.Value;
                break;
    
            // Числовые параметры (int)
            case "opponents": // В скрипте Opponents, в коде Opponent
                _config.Opponent = ParseInt(i.Value);
                break;
            case "cashvalue":
                _config.CashValue = ParseInt(i.Value);
                break;
            case "trafficlevel":
                _config.TrafficLevel = ParseInt(i.Value);
                break;
            case "racelength":
                _config.RaceLength = ParseInt(i.Value);
                break;
            case "numlaps":
                _config.NumLaps = ParseInt(i.Value);
                break;
            case "chanceofrain":
                _config.ChanceOfRain = ParseInt(i.Value);
                break;
            case "timelimit":
                _config.TimeLimit = ParseInt(i.Value);
                break;
            case "reputation":
                _config.Reputation = ParseInt(i.Value);
                break;
    
            // Числовые (double/float)
            case "rivalbesttime":
                _config.RivalBestTime = ParseFloat(i.Value);
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