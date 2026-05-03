using Maximus.Models;

namespace Maximus.Services;

public class TollboothRaceCodeGenerator : BaseCodeGenerator
{
    public TollboothRaceCodeGenerator(RaceConfig config) : base(config) { }

    protected override void ParseSpecificSettings()
    {
        builder.SetChildrenEntry();
        builder.SetTimeLimit(config.TimeLimit);
        var tbc = config.Checkpoints.Where(c => c.Name.Contains('/'));
        foreach (var timeBonusCheckpoint in tbc)
        {
            builder.AddChildNode(EntityType.timebonuscheckpoint, timeBonusCheckpoint.Name.Replace("/", "time_bonus_"), new Dictionary<string, object>
            {
                {"Position", new Dictionary<string, object>
                    {
                        {"X", timeBonusCheckpoint.Point.PositionX},
                        {"Y", timeBonusCheckpoint.Point.PositionY},
                        {"Z", timeBonusCheckpoint.Point.PositionZ}
                    }
                },
                {"Rotation", timeBonusCheckpoint.Point.Rotation},
                {"TimeBonus", timeBonusCheckpoint.TimeBonus},
                {"Template", ""}
            });
        }
    }
}