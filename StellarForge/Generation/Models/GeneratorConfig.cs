namespace StellarForge.Generation.Models;

public class GeneratorConfig
{
    public string SeedText { get; set; } = "DefaultSeed";
    public string SystemName { get; set; } = "Generated System";
    public int MinPlanets { get; set; } = 3;
    public int MaxPlanets { get; set; } = 10;
    public float HabitabilityBias { get; set; } = 0.3f;
    public float MoonFrequency { get; set; } = 0.5f;
    public float GasGiantChance { get; set; } = 0.4f;
    public int TextureResolution { get; set; } = 1024;
    public bool UseGpu { get; set; } = true;
}
