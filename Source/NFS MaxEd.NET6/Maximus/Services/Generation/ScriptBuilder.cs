using System.Text;
using Maximus.Services.IR;
using Maximus.Services.IR.Renderers;

namespace Maximus.Services;

public abstract class ScriptBuilder
{
    protected ScriptDoc Doc = new();
    protected BaseRenderer Renderer;
    public string Build() => Renderer.Render(Doc).Trim().Replace(",", ".");
}