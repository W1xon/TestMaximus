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
        
        config.NodeType = ParseRaceType(addNodes[0].Subject);
        var updateFields = doc.Instructions.Where(i => i.Type == InstrucionType.UpdateField).ToList();

        var barrierFields = updateFields.Where(i => i.Subject.Contains("Barriers")).ToList();
        foreach (var field in barrierFields)
            HandleBarrier(config, field);
        
        var checkpointFields = updateFields.Where(i => i.Path.Contains("checkpoint")).ToList();
        foreach(var field in checkpointFields)
            HandleCheckpoint(config, field);
    }

    private void HandleCheckpoint(RaceConfig config, ScriptInstrucion field)
    {
        string name = field.Path.Split('/').Last();
        int index = ParseInt(name.Substring("checkpoint".Length));
        CheckpointEntity checkpoint;
        if(index >= config.Checkpoints.Count)
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