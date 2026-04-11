using Maximus.Models;

namespace Maximus.Services;

public class CircuitParser : BaseParser
{
    public CircuitParser(RaceConfig config) : base(config) { }

    protected override void ParseSpecificSettings()
    {
        builder.SetNumLaps(config.NumLaps);
    }
}