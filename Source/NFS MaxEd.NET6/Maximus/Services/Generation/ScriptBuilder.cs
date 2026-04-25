using System.Text;
using Maximus.Services.IR;

namespace Maximus.Services;

public abstract class ScriptBuilder
{
    protected ScriptDoc Doc = new();
    public abstract string Build();
}