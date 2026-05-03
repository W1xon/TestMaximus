using Maximus.Models;

namespace Maximus.Services;

public class CircuitCodeGenerator : BaseCodeGenerator
{
    public CircuitCodeGenerator(RaceConfig config) : base(config) { }

    protected override void ParseSpecificSettings()
    {
        builder.SetNumLaps(config.NumLaps);
    }
}