using Maximus.Models;

namespace Maximus.Services;

public class RaceParserFactory
{
    public static BaseCodeGenerator CreateRaceParser(RaceConfig config)
    {
        return config.NodeType switch
        {
            RaceType.circuit => new CircuitCodeGenerator(config),
            RaceType.p2p => new P2PCodeGenerator(config),
            RaceType.lapknockout => new LapknockoutCodeGenerator(config),
            RaceType.tollboothrace => new TollboothRaceCodeGenerator(config),
            RaceType.speedtraprace => new SpeedtrapRaceCodeGenerator(config),
            RaceType.cashgrab => new CashgrabCodeGenerator(config),
            RaceType.drag => new DragCodeGenerator(config),
            _ => throw new ArgumentException($"Unsupported NodeType: {config.NodeType}")
        };
    }
}
