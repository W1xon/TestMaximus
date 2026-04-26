using System.Text;

namespace Maximus.Services.IR.Renderers;

public abstract class BaseRenderer
{
    protected readonly StringBuilder Sb = new();
    public abstract string Render(ScriptDoc doc);
    protected abstract string HandleInstruction(ScriptInstrucion i);
}