using System.Text;

namespace Maximus.Services;

public abstract class ScriptBuilder
{
    protected StringBuilder _commands = new();
    public abstract string Build();
}