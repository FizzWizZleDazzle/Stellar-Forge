using StellarForge.Generation;
using StellarForge.Generation.Models;
using Xunit;

namespace StellarForge.Tests;

public class DeterminismTests
{
    [Fact]
    public void SameSeed_ProducesIdenticalSystems()
    {
        var config = new GeneratorConfig { SeedText = "TestSeed42", MinPlanets = 4, MaxPlanets = 8 };

        var system1 = SystemGenerator.Generate(config);
        var system2 = SystemGenerator.Generate(config);

        Assert.Equal(system1.Star.Id, system2.Star.Id);
        Assert.Equal(system1.Star.MassKg, system2.Star.MassKg);
        Assert.Equal(system1.Star.SpectralType, system2.Star.SpectralType);
        Assert.Equal(system1.Planets.Count, system2.Planets.Count);

        for (int i = 0; i < system1.Planets.Count; i++)
        {
            Assert.Equal(system1.Planets[i].Id, system2.Planets[i].Id);
            Assert.Equal(system1.Planets[i].PlanetType, system2.Planets[i].PlanetType);
            Assert.Equal(system1.Planets[i].MassKg, system2.Planets[i].MassKg);
            Assert.Equal(system1.Planets[i].SemiMajorAxisKm, system2.Planets[i].SemiMajorAxisKm);
            Assert.Equal(system1.Planets[i].Moons.Count, system2.Planets[i].Moons.Count);
        }
    }

    [Fact]
    public void DifferentSeeds_ProduceDifferentSystems()
    {
        var config1 = new GeneratorConfig { SeedText = "Alpha" };
        var config2 = new GeneratorConfig { SeedText = "Beta" };

        var system1 = SystemGenerator.Generate(config1);
        var system2 = SystemGenerator.Generate(config2);

        // Very unlikely to generate identical systems from different seeds
        bool anyDifference = system1.Star.SpectralType != system2.Star.SpectralType
            || system1.Planets.Count != system2.Planets.Count
            || system1.Star.MassKg != system2.Star.MassKg;

        Assert.True(anyDifference, "Different seeds should produce different systems");
    }

    [Fact]
    public void SeededRandom_IsDeterministic()
    {
        var rng1 = new SeededRandom(12345);
        var rng2 = new SeededRandom(12345);

        for (int i = 0; i < 100; i++)
        {
            Assert.Equal(rng1.NextDouble(), rng2.NextDouble());
        }
    }

    [Fact]
    public void SeededRandom_GaussianProducesValues()
    {
        var rng = new SeededRandom(99);
        var values = Enumerable.Range(0, 1000).Select(_ => rng.NextGaussian(0, 1)).ToList();

        // Mean should be roughly 0
        double mean = values.Average();
        Assert.InRange(mean, -0.2, 0.2);

        // Should have some spread
        double maxVal = values.Max();
        double minVal = values.Min();
        Assert.True(maxVal > 1.0);
        Assert.True(minVal < -1.0);
    }
}
