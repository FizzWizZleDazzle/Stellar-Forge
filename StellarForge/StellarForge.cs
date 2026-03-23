using KSA;
using StarMap.API;

namespace StellarForge;

[StarMapMod]
public class StellarForge
{
    private GeneratorUi? _ui;
    private string _contentPath = "";

    [StarMapImmediateLoad]
    public void Init(Mod definingMod)
    {
        _contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "StellarForge");
        Directory.CreateDirectory(_contentPath);

        _ui = new GeneratorUi();
        _ui.SetOutputPath(_contentPath);
    }

    [StarMapAfterGui]
    public void AfterGUI(double dt)
    {
        _ui?.Draw();
    }

    [StarMapAllModsLoaded]
    public void FullyLoaded()
    {
        // Harmony patches could go here if needed
    }

    [StarMapUnload]
    public void Unload()
    {
        _ui = null;
    }
}
