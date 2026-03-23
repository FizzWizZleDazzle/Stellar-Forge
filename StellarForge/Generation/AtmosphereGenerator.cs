using StellarForge.Generation.Models;

namespace StellarForge.Generation;

public static class AtmosphereGenerator
{
    public static AtmosphereData? Generate(SeededRandom rng, PlanetType type, double radiusKm)
    {
        // Dwarf planets and small rocky bodies rarely have atmosphere
        if (type == PlanetType.Dwarf) return null;
        if (type == PlanetType.Rocky && !rng.Chance(0.3)) return null;

        var atmo = new AtmosphereData();

        switch (type)
        {
            case PlanetType.OceanWorld:
                // Earth-like: blue Rayleigh scattering
                atmo.RayleighR = (float)rng.NextDouble(0.004, 0.008);
                atmo.RayleighG = (float)rng.NextDouble(0.010, 0.018);
                atmo.RayleighB = (float)rng.NextDouble(0.025, 0.040);
                atmo.RayleighScaleHeightKm = (float)rng.NextDouble(6, 10);
                atmo.MieR = atmo.MieG = atmo.MieB = (float)rng.NextDouble(0.001, 0.003);
                atmo.MieScaleHeightKm = (float)rng.NextDouble(1.2, 2.5);
                atmo.MiePhaseAsymmetry = (float)rng.NextDouble(0.7, 0.9);
                atmo.MieAbsorptionMultiplier = (float)rng.NextDouble(0.9, 1.2);
                atmo.OzoneR = 0.000477f;
                atmo.OzoneG = 0.001154f;
                atmo.OzoneB = 0.0000501f;
                atmo.OzoneAltitudeKm = 20;
                atmo.OzoneExtentKm = 10;
                atmo.SeaLevelPressureAtm = (float)rng.NextDouble(0.5, 2.0);
                atmo.SeaLevelDensityKgPerM3 = (float)(atmo.SeaLevelPressureAtm * 1.225);
                atmo.PhysicalScaleHeightKm = atmo.RayleighScaleHeightKm;
                break;

            case PlanetType.Rocky:
                // Thin, dusty atmosphere (Mars-like)
                atmo.RayleighR = (float)rng.NextDouble(0.015, 0.025);
                atmo.RayleighG = (float)rng.NextDouble(0.008, 0.015);
                atmo.RayleighB = (float)rng.NextDouble(0.003, 0.008);
                atmo.RayleighScaleHeightKm = (float)rng.NextDouble(8, 14);
                atmo.MieR = atmo.MieG = atmo.MieB = (float)rng.NextDouble(0.003, 0.008);
                atmo.MieScaleHeightKm = (float)rng.NextDouble(1.5, 4.0);
                atmo.MiePhaseAsymmetry = (float)rng.NextDouble(0.6, 0.85);
                atmo.MieAbsorptionMultiplier = (float)rng.NextDouble(0.8, 1.5);
                atmo.SeaLevelPressureAtm = (float)rng.NextDouble(0.001, 0.1);
                atmo.SeaLevelDensityKgPerM3 = (float)(atmo.SeaLevelPressureAtm * 1.225);
                atmo.PhysicalScaleHeightKm = atmo.RayleighScaleHeightKm;
                break;

            case PlanetType.GasGiant:
                // Thick hydrogen atmosphere, warm tones
                atmo.RayleighR = (float)rng.NextDouble(0.010, 0.020);
                atmo.RayleighG = (float)rng.NextDouble(0.008, 0.015);
                atmo.RayleighB = (float)rng.NextDouble(0.004, 0.010);
                atmo.RayleighScaleHeightKm = (float)(radiusKm * rng.NextDouble(0.002, 0.005));
                atmo.MieR = atmo.MieG = atmo.MieB = (float)rng.NextDouble(0.005, 0.015);
                atmo.MieScaleHeightKm = atmo.RayleighScaleHeightKm * 0.3f;
                atmo.MiePhaseAsymmetry = (float)rng.NextDouble(0.75, 0.95);
                atmo.MieAbsorptionMultiplier = (float)rng.NextDouble(1.0, 2.0);
                atmo.SeaLevelPressureAtm = (float)rng.NextDouble(10, 1000);
                atmo.SeaLevelDensityKgPerM3 = (float)rng.NextDouble(0.1, 1.0);
                atmo.PhysicalScaleHeightKm = atmo.RayleighScaleHeightKm;
                break;

            case PlanetType.IceGiant:
                // Methane-rich, blue-green tones
                atmo.RayleighR = (float)rng.NextDouble(0.003, 0.007);
                atmo.RayleighG = (float)rng.NextDouble(0.010, 0.020);
                atmo.RayleighB = (float)rng.NextDouble(0.015, 0.030);
                atmo.RayleighScaleHeightKm = (float)(radiusKm * rng.NextDouble(0.002, 0.004));
                atmo.MieR = atmo.MieG = atmo.MieB = (float)rng.NextDouble(0.003, 0.010);
                atmo.MieScaleHeightKm = atmo.RayleighScaleHeightKm * 0.25f;
                atmo.MiePhaseAsymmetry = (float)rng.NextDouble(0.7, 0.9);
                atmo.MieAbsorptionMultiplier = (float)rng.NextDouble(0.9, 1.5);
                atmo.SeaLevelPressureAtm = (float)rng.NextDouble(5, 500);
                atmo.SeaLevelDensityKgPerM3 = (float)rng.NextDouble(0.1, 0.8);
                atmo.PhysicalScaleHeightKm = atmo.RayleighScaleHeightKm;
                break;
        }

        atmo.StartHeightKm = 0;
        return atmo;
    }
}
