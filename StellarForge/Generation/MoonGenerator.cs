using StellarForge.Generation.Models;

namespace StellarForge.Generation;

public static class MoonGenerator
{
    public static void GenerateMoons(SeededRandom rng, PlanetData planet, StarData star, GeneratorConfig config)
    {
        int maxSlots = planet.PlanetType switch
        {
            PlanetType.GasGiant => 5,
            PlanetType.IceGiant => 4,
            PlanetType.Rocky => 2,
            PlanetType.OceanWorld => 2,
            PlanetType.Dwarf => 1,
            _ => 0
        };

        double hillRadius = OrbitalMechanics.HillSphereRadius(planet.SemiMajorAxisKm, planet.MassKg, star.MassKg);
        double minSma = planet.MeanRadiusKm * 3.0; // at least 3x planet radius
        double maxSma = hillRadius / 3.0;           // 1/3 of Hill sphere

        if (maxSma <= minSma) return; // planet too small or too close to star

        int moonCount = 0;
        for (int i = 0; i < maxSlots; i++)
        {
            if (rng.Chance(config.MoonFrequency))
                moonCount++;
        }

        if (moonCount == 0) return;

        // Logarithmically space moon orbits
        double logMin = Math.Log(minSma);
        double logMax = Math.Log(maxSma);

        for (int i = 0; i < moonCount; i++)
        {
            double t = moonCount == 1 ? 0.5 : (double)i / (moonCount - 1);
            double smaKm = Math.Exp(logMin + t * (logMax - logMin)) * rng.NextDouble(0.9, 1.1);

            // Mass: fraction of parent
            double massFraction = planet.PlanetType switch
            {
                PlanetType.GasGiant => rng.NextDouble(0.00001, 0.001),
                PlanetType.IceGiant => rng.NextDouble(0.00005, 0.005),
                _ => rng.NextDouble(0.001, 0.05)
            };
            double massKg = planet.MassKg * massFraction;

            // Radius from mass assuming density ~3500 kg/m³
            double density = 3500.0;
            double volumeM3 = massKg / density;
            double radiusM = Math.Pow(3.0 * volumeM3 / (4.0 * Math.PI), 1.0 / 3.0);
            double radiusKm = radiusM / 1000.0;

            // Orbital elements
            double ecc = rng.NextDouble(0.0, 0.1);
            double inc = rng.NextDouble(0.0, 10.0);
            double lan = rng.NextDouble(0.0, 360.0);
            double aop = rng.NextDouble(0.0, 360.0);
            double period = OrbitalMechanics.OrbitalPeriod(smaKm, planet.MassKg);
            double tap = rng.NextDouble(-period / 2.0, period / 2.0);

            bool tidallyLocked = smaKm < planet.MeanRadiusKm * 20.0;

            string moonId = $"{planet.Id}_Moon_{i}";
            string moonName = NameGenerator.GenerateMoonName(rng, planet.Name, i);

            planet.Moons.Add(new MoonData
            {
                Id = moonId,
                Name = moonName,
                ParentId = planet.Id,
                SemiMajorAxisKm = smaKm,
                Eccentricity = ecc,
                InclinationDeg = inc,
                LongitudeOfAscendingNodeDeg = lan,
                ArgumentOfPeriapsisDeg = aop,
                TimeAtPeriapsisSeconds = tap,
                MassKg = massKg,
                MeanRadiusKm = Math.Max(radiusKm, 10), // min 10km
                IsTidallyLocked = tidallyLocked,
                SiderealPeriodHours = tidallyLocked ? period / 3600.0 : rng.NextDouble(5, 50),
                TiltDeg = rng.NextDouble(0, 15),
                ColorR = (float)rng.NextDouble(0.4, 0.9),
                ColorG = (float)rng.NextDouble(0.4, 0.9),
                ColorB = (float)rng.NextDouble(0.4, 0.9),
                DiffuseTexturePath = $"Textures/{moonId}_Diffuse.png",
                NormalTexturePath = $"Textures/{moonId}_Normal.png",
                HeightTexturePath = $"Textures/{moonId}_Height.png",
                HeightMinKm = -rng.NextDouble(0.01, 0.5),
                HeightMaxKm = rng.NextDouble(0.1, 3.0),
            });
        }
    }
}
