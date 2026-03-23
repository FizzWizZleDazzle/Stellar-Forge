using StellarForge.Generation.Models;

namespace StellarForge.Generation;

public static class StarGenerator
{
    private record SpectralTypeData(
        SpectralType Type, double Weight,
        double MinMassSol, double MaxMassSol,
        double MinRadiusSol, double MaxRadiusSol,
        double MinTempK, double MaxTempK,
        double MinLumSol, double MaxLumSol,
        float R, float G, float B);

    private static readonly SpectralTypeData[] SpectralTable =
    {
        new(SpectralType.O, 0.003, 16, 150, 6.6, 20, 30000, 52000, 30000, 1000000, 0.61f, 0.69f, 1.0f),
        new(SpectralType.B, 0.01, 2.1, 16, 1.8, 6.6, 10000, 30000, 25, 30000, 0.67f, 0.77f, 1.0f),
        new(SpectralType.A, 0.02, 1.4, 2.1, 1.4, 1.8, 7500, 10000, 5, 25, 0.79f, 0.84f, 1.0f),
        new(SpectralType.F, 0.05, 1.04, 1.4, 1.15, 1.4, 6000, 7500, 1.5, 5, 0.96f, 0.94f, 0.91f),
        new(SpectralType.G, 0.10, 0.8, 1.04, 0.96, 1.15, 5200, 6000, 0.6, 1.5, 1.0f, 0.96f, 0.84f),
        new(SpectralType.K, 0.15, 0.45, 0.8, 0.7, 0.96, 3700, 5200, 0.08, 0.6, 1.0f, 0.83f, 0.56f),
        new(SpectralType.M, 0.65, 0.08, 0.45, 0.15, 0.7, 2400, 3700, 0.001, 0.08, 1.0f, 0.70f, 0.42f),
    };

    private const double SOLAR_MASS_KG = 1.989e30;
    private const double SOLAR_RADIUS_KM = 695700.0;

    public static StarData Generate(SeededRandom rng, string nameId)
    {
        var weights = SpectralTable.Select(s => s.Weight).ToArray();
        int idx = rng.WeightedPick(weights);
        var spec = SpectralTable[idx];

        double t = rng.NextDouble();
        double massSol = spec.MinMassSol + t * (spec.MaxMassSol - spec.MinMassSol);
        double radiusSol = spec.MinRadiusSol + t * (spec.MaxRadiusSol - spec.MinRadiusSol);
        double tempK = spec.MinTempK + t * (spec.MaxTempK - spec.MinTempK);
        double lumSol = spec.MinLumSol + t * (spec.MaxLumSol - spec.MinLumSol);

        return new StarData
        {
            Id = nameId,
            Name = nameId,
            SpectralType = spec.Type,
            MassKg = massSol * SOLAR_MASS_KG,
            MeanRadiusKm = radiusSol * SOLAR_RADIUS_KM,
            LuminositySols = lumSol,
            TemperatureK = tempK,
            ColorR = spec.R,
            ColorG = spec.G,
            ColorB = spec.B,
            DiffuseTexturePath = $"Textures/{nameId}_Diffuse.png"
        };
    }
}
