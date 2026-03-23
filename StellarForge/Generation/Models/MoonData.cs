namespace StellarForge.Generation.Models;

public class MoonData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ParentId { get; set; } = "";

    // Orbital elements
    public double SemiMajorAxisKm { get; set; }
    public double Eccentricity { get; set; }
    public double InclinationDeg { get; set; }
    public double LongitudeOfAscendingNodeDeg { get; set; }
    public double ArgumentOfPeriapsisDeg { get; set; }
    public double TimeAtPeriapsisSeconds { get; set; }

    // Physical
    public double MassKg { get; set; }
    public double MeanRadiusKm { get; set; }

    // Rotation
    public bool IsTidallyLocked { get; set; }
    public double SiderealPeriodHours { get; set; }
    public double TiltDeg { get; set; }

    // Appearance
    public float ColorR { get; set; }
    public float ColorG { get; set; }
    public float ColorB { get; set; }

    // Textures
    public string DiffuseTexturePath { get; set; } = "";
    public string NormalTexturePath { get; set; } = "";
    public string HeightTexturePath { get; set; } = "";
    public double HeightMinKm { get; set; }
    public double HeightMaxKm { get; set; }
}
