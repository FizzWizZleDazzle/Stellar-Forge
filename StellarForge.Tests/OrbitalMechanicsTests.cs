using StellarForge.Generation;
using Xunit;

namespace StellarForge.Tests;

public class OrbitalMechanicsTests
{
    [Fact]
    public void HillSphere_EarthSun_ReturnsReasonableValue()
    {
        // Earth-Sun Hill sphere should be ~1.5 million km
        double smaKm = OrbitalMechanics.AU_KM; // 1 AU
        double earthMass = OrbitalMechanics.EARTH_MASS_KG;
        double solarMass = OrbitalMechanics.SOLAR_MASS_KG;

        double hillRadius = OrbitalMechanics.HillSphereRadius(smaKm, earthMass, solarMass);

        Assert.InRange(hillRadius, 1_000_000, 2_000_000); // ~1.5M km
    }

    [Fact]
    public void OrbitalPeriod_Earth_ReturnsOneYear()
    {
        double smaKm = OrbitalMechanics.AU_KM;
        double solarMass = OrbitalMechanics.SOLAR_MASS_KG;

        double periodSeconds = OrbitalMechanics.OrbitalPeriod(smaKm, solarMass);
        double periodDays = periodSeconds / 86400.0;

        Assert.InRange(periodDays, 360, 370); // ~365.25 days
    }

    [Fact]
    public void AreOrbitsStable_WellSeparated_ReturnsTrue()
    {
        double sma1 = OrbitalMechanics.AuToKm(1.0);  // Earth
        double sma2 = OrbitalMechanics.AuToKm(1.52); // Mars
        double mass1 = OrbitalMechanics.EARTH_MASS_KG;
        double mass2 = OrbitalMechanics.EARTH_MASS_KG * 0.107;
        double parentMass = OrbitalMechanics.SOLAR_MASS_KG;

        Assert.True(OrbitalMechanics.AreOrbitsStable(sma1, mass1, sma2, mass2, parentMass));
    }

    [Fact]
    public void AreOrbitsStable_OverlappingOrbits_ReturnsFalse()
    {
        double sma1 = OrbitalMechanics.AuToKm(1.0);
        double sma2 = OrbitalMechanics.AuToKm(1.001); // nearly same orbit
        double mass1 = OrbitalMechanics.JUPITER_MASS_KG;
        double mass2 = OrbitalMechanics.JUPITER_MASS_KG;
        double parentMass = OrbitalMechanics.SOLAR_MASS_KG;

        Assert.False(OrbitalMechanics.AreOrbitsStable(sma1, mass1, sma2, mass2, parentMass));
    }

    [Fact]
    public void HabitableZone_SunLike_ReasonableRange()
    {
        double inner = OrbitalMechanics.HabitableZoneInnerAU(1.0);
        double outer = OrbitalMechanics.HabitableZoneOuterAU(1.0);

        Assert.InRange(inner, 0.8, 1.0);
        Assert.InRange(outer, 1.2, 1.5);
        Assert.True(outer > inner);
    }

    [Fact]
    public void FrostLine_SunLike_BeyondHZ()
    {
        double frostLine = OrbitalMechanics.FrostLineAU(1.0);
        double hzOuter = OrbitalMechanics.HabitableZoneOuterAU(1.0);

        Assert.True(frostLine > hzOuter);
        Assert.InRange(frostLine, 2.0, 4.0);
    }

    [Fact]
    public void AuKmConversion_Roundtrips()
    {
        double original = 3.5;
        double km = OrbitalMechanics.AuToKm(original);
        double back = OrbitalMechanics.KmToAu(km);

        Assert.Equal(original, back, 6);
    }
}
