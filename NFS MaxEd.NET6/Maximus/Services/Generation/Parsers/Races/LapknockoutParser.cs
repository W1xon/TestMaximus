using Maximus.Models;

namespace Maximus.Services;

public class LapknockoutParser : BaseParser
{
    public LapknockoutParser(RaceConfig config) : base(config) { }

    protected override void ParseSpecificSettings()
    {
        builder.SetNumLaps(config.NumLaps);
    }
}