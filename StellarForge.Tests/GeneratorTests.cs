using StellarForge.Generation;
using StellarForge.Generation.Models;
using Xunit;

namespace StellarForge.Tests;

public class GeneratorTests
{
    [Fact]
    public void StarGenerator_ProducesValidStar()
    {
        var rng = new SeededRandom(42);
        var star = StarGenerator.Generate(rng, "TestStar");

        Assert.Equal("TestStar", star.Id);
        Assert.True(star.MassKg > 0);
        Assert.True(star.MeanRadiusKm > 0);
        Assert.True(star.LuminositySols > 0);
        Assert.True(star.TemperatureK > 0);
        Assert.InRange(star.ColorR, 0, 1);
        Assert.InRange(star.ColorG, 0, 1);
        Assert.InRange(star.ColorB, 0, 1);
    }

    [Fact]
    public void PlanetGenerator_RespectsMinMaxCount()
    {
        var rng = new SeededRandom(42);
        var star = StarGenerator.Generate(rng, "Star");
        var config = new GeneratorConfig { MinPlanets = 3, MaxPlanets = 5 };

        // Run multiple times to check bounds
        for (int seed = 0; seed < 20; seed++)
        {
            var testRng = new SeededRandom(seed);
            var testStar = StarGenerator.Generate(testRng, "Star");
            var planets = PlanetGenerator.Generate(testRng, testStar, config);

            Assert.InRange(planets.Count, 3, 5);
        }
    }

    [Fact]
    public void PlanetGenerator_AllOrbitalElementsValid()
    {
        var rng = new SeededRandom(42);
        var star = StarGenerator.Generate(rng, "Star");
        var config = new GeneratorConfig { MinPlanets = 5, MaxPlanets = 10 };
        var planets = PlanetGenerator.Generate(rng, star, config);

        foreach (var planet in planets)
        {
            Assert.True(planet.SemiMajorAxisKm > 0, $"Planet {planet.Id} SMA must be positive");
            Assert.InRange(planet.Eccentricity, 0, 1);
            Assert.InRange(planet.InclinationDeg, 0, 360);
            Assert.True(planet.MassKg > 0);
            Assert.True(planet.MeanRadiusKm > 0);
        }
    }

    [Fact]
    public void MoonGenerator_MoonsWithinHillSphere()
    {
        var config = new GeneratorConfig { MoonFrequency = 1.0f }; // maximize moons
        var rng = new SeededRandom(42);
        var star = StarGenerator.Generate(rng, "Star");
        var planets = PlanetGenerator.Generate(rng, star, config);

        foreach (var planet in planets)
        {
            MoonGenerator.GenerateMoons(rng, planet, star, config);
            double hillRadius = OrbitalMechanics.HillSphereRadius(planet.SemiMajorAxisKm, planet.MassKg, star.MassKg);

            foreach (var moon in planet.Moons)
            {
                Assert.True(moon.SemiMajorAxisKm < hillRadius,
                    $"Moon {moon.Id} SMA ({moon.SemiMajorAxisKm:N0} km) exceeds Hill sphere ({hillRadius:N0} km)");
                Assert.True(moon.SemiMajorAxisKm > planet.MeanRadiusKm,
                    $"Moon {moon.Id} SMA must be greater than planet radius");
            }
        }
    }

    [Fact]
    public void SystemGenerator_ProducesCompleteSystem()
    {
        var config = new GeneratorConfig { SeedText = "CompleteTest", MinPlanets = 4, MaxPlanets = 8 };
        var system = SystemGenerator.Generate(config);

        Assert.NotEmpty(system.SystemId);
        Assert.NotEmpty(system.DisplayName);
        Assert.NotNull(system.Star);
        Assert.True(system.Planets.Count >= 4);
        Assert.True(system.Planets.Count <= 8);

        // All planets reference the star as parent
        foreach (var planet in system.Planets)
        {
            Assert.Equal(system.Star.Id, planet.ParentId);
            Assert.NotEmpty(planet.DiffuseTexturePath);

            // All moons reference their planet
            foreach (var moon in planet.Moons)
            {
                Assert.Equal(planet.Id, moon.ParentId);
            }
        }
    }

    [Fact]
    public void NameGenerator_ProducesNonEmptyNames()
    {
        var rng = new SeededRandom(42);

        string starName = NameGenerator.GenerateStarName(rng);
        Assert.False(string.IsNullOrWhiteSpace(starName));

        string planetName = NameGenerator.GeneratePlanetName(rng, starName, 0);
        Assert.False(string.IsNullOrWhiteSpace(planetName));

        string moonName = NameGenerator.GenerateMoonName(rng, planetName, 0);
        Assert.False(string.IsNullOrWhiteSpace(moonName));
    }

    [Theory]
    [InlineData(PlanetType.OceanWorld)]
    [InlineData(PlanetType.GasGiant)]
    [InlineData(PlanetType.IceGiant)]
    public void AtmosphereGenerator_TypesWithAtmosphere_ProduceValidData(PlanetType type)
    {
        var rng = new SeededRandom(42);
        // Generate multiple times since Rocky has only 30% chance
        AtmosphereData? atmo = null;
        for (int i = 0; i < 100 && atmo == null; i++)
        {
            atmo = AtmosphereGenerator.Generate(new SeededRandom(i), type, 6371);
        }

        Assert.NotNull(atmo);
        Assert.True(atmo!.RayleighScaleHeightKm > 0);
        Assert.True(atmo.MieScaleHeightKm > 0);
        Assert.True(atmo.SeaLevelPressureAtm > 0);
    }
}
