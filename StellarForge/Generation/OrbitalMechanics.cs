namespace StellarForge.Generation;

public static class OrbitalMechanics
{
    public const double G = 6.67430e-11; // m^3 kg^-1 s^-2
    public const double AU_KM = 1.495978707e8; // km per AU
    public const double SOLAR_MASS_KG = 1.989e30;
    public const double EARTH_MASS_KG = 5.972e24;
    public const double JUPITER_MASS_KG = 1.898e27;

    /// <summary>Hill sphere radius in km</summary>
    public static double HillSphereRadius(double smaKm, double bodyMassKg, double parentMassKg)
    {
        return smaKm * Math.Pow(bodyMassKg / (3.0 * parentMassKg), 1.0 / 3.0);
    }

    /// <summary>Check if two orbits are stable using mutual Hill radius criterion (separation > 3.46 * mutual Hill radius)</summary>
    public static bool AreOrbitsStable(double sma1Km, double mass1Kg, double sma2Km, double mass2Kg, double parentMassKg)
    {
        double mutualHill = 0.5 * (sma1Km + sma2Km) * Math.Pow((mass1Kg + mass2Kg) / (3.0 * parentMassKg), 1.0 / 3.0);
        double separation = Math.Abs(sma2Km - sma1Km);
        return separation > 3.46 * mutualHill;
    }

    /// <summary>Orbital period in seconds, given SMA in km and parent mass in kg</summary>
    public static double OrbitalPeriod(double smaKm, double parentMassKg)
    {
        double smaM = smaKm * 1000.0;
        return 2.0 * Math.PI * Math.Sqrt(smaM * smaM * smaM / (G * parentMassKg));
    }

    /// <summary>Roche limit in km (fluid body approximation)</summary>
    public static double RocheLimit(double primaryRadiusKm, double primaryDensity, double secondaryDensity)
    {
        return 2.44 * primaryRadiusKm * Math.Pow(primaryDensity / secondaryDensity, 1.0 / 3.0);
    }

    /// <summary>Habitable zone inner edge in AU (simplified)</summary>
    public static double HabitableZoneInnerAU(double luminositySols)
    {
        return 0.95 * Math.Sqrt(luminositySols);
    }

    /// <summary>Habitable zone outer edge in AU (simplified)</summary>
    public static double HabitableZoneOuterAU(double luminositySols)
    {
        return 1.37 * Math.Sqrt(luminositySols);
    }

    /// <summary>Frost line in AU</summary>
    public static double FrostLineAU(double luminositySols)
    {
        return 2.7 * Math.Sqrt(luminositySols);
    }

    public static double AuToKm(double au) => au * AU_KM;
    public static double KmToAu(double km) => km / AU_KM;
}
