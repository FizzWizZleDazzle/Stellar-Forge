using StellarForge.Generation.Models;

namespace StellarForge.Generation;

public static class PlanetGenerator
{
    private record PlanetTypeProfile(double MinMassEarths, double MaxMassEarths, double MinRadiusKm, double MaxRadiusKm);

    private static readonly Dictionary<PlanetType, PlanetTypeProfile> Profiles = new()
    {
        [PlanetType.Rocky] = new(0.1, 3.0, 2000, 8000),
        [PlanetType.OceanWorld] = new(0.5, 5.0, 4000, 10000),
        [PlanetType.GasGiant] = new(15, 4000, 20000, 80000),
        [PlanetType.IceGiant] = new(5, 20, 15000, 30000),
        [PlanetType.Dwarf] = new(0.001, 0.1, 200, 2000),
    };

    public static List<PlanetData> Generate(SeededRandom rng, StarData star, GeneratorConfig config)
    {
        int count = rng.Next(config.MinPlanets, config.MaxPlanets + 1);
        var planets = new List<PlanetData>();

        // Titius-Bode spacing
        double a0 = 0.3 * Math.Sqrt(star.LuminositySols); // base distance in AU, scales with luminosity
        double k = rng.NextDouble(1.5, 2.5);

        double hzInner = OrbitalMechanics.HabitableZoneInnerAU(star.LuminositySols);
        double hzOuter = OrbitalMechanics.HabitableZoneOuterAU(star.LuminositySols);
        double frostLine = OrbitalMechanics.FrostLineAU(star.LuminositySols);

        for (int i = 0; i < count; i++)
        {
            double jitter = rng.NextDouble(0.9, 1.1);
            double smaAU = a0 * Math.Pow(k, i) * jitter;
            double smaKm = OrbitalMechanics.AuToKm(smaAU);

            // Type selection based on zone
            PlanetType type = SelectType(rng, smaAU, hzInner, hzOuter, frostLine, config);

            var profile = Profiles[type];
            double t = rng.NextDouble();
            double massEarths = profile.MinMassEarths + t * (profile.MaxMassEarths - profile.MinMassEarths);
            double massKg = massEarths * OrbitalMechanics.EARTH_MASS_KG;
            double radiusKm = profile.MinRadiusKm + t * (profile.MaxRadiusKm - profile.MinRadiusKm);

            // Orbital elements
            double ecc = type == PlanetType.GasGiant ? rng.NextDouble(0.0, 0.1) : rng.NextDouble(0.0, 0.2);
            double inc = rng.NextDouble(0.0, 5.0);
            double lan = rng.NextDouble(0.0, 360.0);
            double aop = rng.NextDouble(0.0, 360.0);
            double mae = rng.NextDouble(0.0, 360.0);
            double period = OrbitalMechanics.OrbitalPeriod(smaKm, star.MassKg);
            double tap = rng.NextDouble(-period / 2.0, period / 2.0);

            // Rotation
            bool tidallyLocked = smaAU < 0.1;
            double siderealHours = tidallyLocked ? (period / 3600.0) : rng.NextDouble(8, 100);
            double tilt = rng.NextDouble(0, 45);

            // Color (golden ratio hue stepping for distinct colors)
            float hue = (float)((i * 0.618033988749895) % 1.0);
            var (cr, cg, cb) = HsvToRgb(hue, 0.5f, 0.9f);

            // Height range
            double heightMax = type switch
            {
                PlanetType.Rocky => rng.NextDouble(1, 15),
                PlanetType.OceanWorld => rng.NextDouble(0.5, 8),
                PlanetType.GasGiant => 0,
                PlanetType.IceGiant => 0,
                PlanetType.Dwarf => rng.NextDouble(0.1, 5),
                _ => 1
            };
            double heightMin = -heightMax * rng.NextDouble(0.05, 0.3);

            string planetId = $"Planet_{i}";
            planets.Add(new PlanetData
            {
                Id = planetId,
                Name = planetId,
                ParentId = star.Id,
                PlanetType = type,
                SemiMajorAxisKm = smaKm,
                Eccentricity = ecc,
                InclinationDeg = inc,
                LongitudeOfAscendingNodeDeg = lan,
                ArgumentOfPeriapsisDeg = aop,
                TimeAtPeriapsisSeconds = tap,
                MeanAnomalyAtEpochDeg = mae,
                MassKg = massKg,
                MeanRadiusKm = radiusKm,
                IsTidallyLocked = tidallyLocked,
                SiderealPeriodHours = siderealHours,
                TiltDeg = tilt,
                AzimuthDeg = 0,
                ColorR = cr,
                ColorG = cg,
                ColorB = cb,
                HeightMinKm = heightMin,
                HeightMaxKm = heightMax,
                DiffuseTexturePath = $"Textures/{planetId}_Diffuse.png",
                NormalTexturePath = $"Textures/{planetId}_Normal.png",
                HeightTexturePath = $"Textures/{planetId}_Height.png",
            });
        }

        // Validate orbital stability and push apart if needed
        ValidateStability(planets, star.MassKg);

        return planets;
    }

    private static PlanetType SelectType(SeededRandom rng, double smaAU, double hzInner, double hzOuter, double frostLine, GeneratorConfig config)
    {
        if (smaAU < hzInner * 0.5)
        {
            // Hot inner zone
            return rng.Chance(0.7) ? PlanetType.Rocky : PlanetType.Dwarf;
        }
        else if (smaAU >= hzInner && smaAU <= hzOuter)
        {
            // Habitable zone
            double oceanChance = 0.2 + config.HabitabilityBias * 0.6; // 0.2 to 0.8
            if (rng.Chance(oceanChance)) return PlanetType.OceanWorld;
            return rng.Chance(0.7) ? PlanetType.Rocky : PlanetType.Dwarf;
        }
        else if (smaAU > hzOuter && smaAU < frostLine)
        {
            // Between HZ and frost line
            return rng.Chance(0.6) ? PlanetType.Rocky : PlanetType.Dwarf;
        }
        else
        {
            // Beyond frost line
            double gasChance = config.GasGiantChance;
            if (rng.Chance(gasChance))
                return rng.Chance(0.6) ? PlanetType.GasGiant : PlanetType.IceGiant;
            return rng.Chance(0.5) ? PlanetType.Rocky : PlanetType.Dwarf;
        }
    }

    private static void ValidateStability(List<PlanetData> planets, double starMassKg)
    {
        planets.Sort((a, b) => a.SemiMajorAxisKm.CompareTo(b.SemiMajorAxisKm));

        for (int attempts = 0; attempts < 10; attempts++)
        {
            bool allStable = true;
            for (int i = 0; i < planets.Count - 1; i++)
            {
                if (!OrbitalMechanics.AreOrbitsStable(
                    planets[i].SemiMajorAxisKm, planets[i].MassKg,
                    planets[i + 1].SemiMajorAxisKm, planets[i + 1].MassKg,
                    starMassKg))
                {
                    // Push outer planet out by 10%
                    planets[i + 1].SemiMajorAxisKm *= 1.1;
                    allStable = false;
                }
            }
            if (allStable) break;
        }
    }

    private static (float r, float g, float b) HsvToRgb(float h, float s, float v)
    {
        int hi = (int)(h * 6) % 6;
        float f = h * 6 - (int)(h * 6);
        float p = v * (1 - s);
        float q = v * (1 - f * s);
        float t = v * (1 - (1 - f) * s);
        return hi switch
        {
            0 => (v, t, p),
            1 => (q, v, p),
            2 => (p, v, t),
            3 => (p, q, v),
            4 => (t, p, v),
            _ => (v, p, q),
        };
    }
}
