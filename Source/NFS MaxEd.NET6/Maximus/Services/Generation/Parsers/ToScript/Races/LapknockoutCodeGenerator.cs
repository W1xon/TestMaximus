using Maximus.Models;

namespace Maximus.Services;

public class LapknockoutCodeGenerator : BaseCodeGenerator
{
    public LapknockoutCodeGenerator(RaceConfig config) : base(config) { }

    protected override void ParseSpecificSettings()
    {
        builder.SetNumLaps(config.NumLaps);
    }
}