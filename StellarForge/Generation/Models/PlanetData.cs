namespace StellarForge.Generation.Models;

public enum PlanetType { Rocky, OceanWorld, GasGiant, IceGiant, Dwarf }

public class PlanetData
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string ParentId { get; set; } = "";
    public PlanetType PlanetType { get; set; }

    // Orbital elements
    public double SemiMajorAxisKm { get; set; }
    public double Eccentricity { get; set; }
    public double InclinationDeg { get; set; }
    public double LongitudeOfAscendingNodeDeg { get; set; }
    public double ArgumentOfPeriapsisDeg { get; set; }
    public double TimeAtPeriapsisSeconds { get; set; }
    public double MeanAnomalyAtEpochDeg { get; set; }

    // Physical
    public double MassKg { get; set; }
    public double MeanRadiusKm { get; set; }

    // Rotation
    public bool IsTidallyLocked { get; set; }
    public double SiderealPeriodHours { get; set; }
    public double TiltDeg { get; set; }
    public double AzimuthDeg { get; set; }

    // Appearance
    public float ColorR { get; set; }
    public float ColorG { get; set; }
    public float ColorB { get; set; }

    // Atmosphere (null = no atmosphere)
    public AtmosphereData? Atmosphere { get; set; }

    // Textures
    public string DiffuseTexturePath { get; set; } = "";
    public string NormalTexturePath { get; set; } = "";
    public string HeightTexturePath { get; set; } = "";
    public double HeightMinKm { get; set; }
    public double HeightMaxKm { get; set; }

    // Moons
    public List<MoonData> Moons { get; set; } = new();

    // Helpers
    public bool HasAtmosphere => Atmosphere != null;
    public string MeshCollectionId => PlanetType switch
    {
        PlanetType.GasGiant => "EarthScale",
        PlanetType.IceGiant => "EarthScale",
        PlanetType.OceanWorld => "OceanEarth",
        PlanetType.Dwarf => "Asteroid",
        _ => "Default"
    };
}
