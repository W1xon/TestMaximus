using Maximus.Models;

namespace Maximus.Services;

public class P2PCodeGenerator : BaseCodeGenerator
{
    public P2PCodeGenerator(RaceConfig config) : base(config) { }

    protected override void ParseSpecificSettings()
    {
    }
}