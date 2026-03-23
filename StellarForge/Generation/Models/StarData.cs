namespace StellarForge.Generation.Models;

public enum SpectralType { O, B, A, F, G, K, M }

public class StarData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public SpectralType SpectralType { get; set; }
    public double MassKg { get; set; }
    public double MeanRadiusKm { get; set; }
    public double LuminositySols { get; set; }
    public double TemperatureK { get; set; }
    public float ColorR { get; set; }
    public float ColorG { get; set; }
    public float ColorB { get; set; }
    public string DiffuseTexturePath { get; set; } = "";
}
