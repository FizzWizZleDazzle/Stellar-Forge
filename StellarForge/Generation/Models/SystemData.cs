namespace StellarForge.Generation.Models;

public class SystemData
{
    public string SystemId { get; set; } = "";
    public string DisplayName { get; set; } = "";
    public string Seed { get; set; } = "";
    public StarData Star { get; set; } = new();
    public List<PlanetData> Planets { get; set; } = new();
}
