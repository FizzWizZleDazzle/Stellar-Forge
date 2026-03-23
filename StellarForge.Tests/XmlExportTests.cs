using System.Xml.Linq;
using StellarForge.Export;
using StellarForge.Generation;
using StellarForge.Generation.Models;
using Xunit;

namespace StellarForge.Tests;

public class XmlExportTests
{
    private SystemData CreateTestSystem()
    {
        var config = new GeneratorConfig
        {
            SeedText = "XmlTest",
            MinPlanets = 3,
            MaxPlanets = 6
        };
        return SystemGenerator.Generate(config);
    }

    [Fact]
    public void AstronomicalsXml_HasCorrectRootElement()
    {
        var system = CreateTestSystem();
        var doc = XmlExporter.Export(system);

        Assert.Equal("Astronomicals", doc.Root!.Name.LocalName);
    }

    [Fact]
    public void AstronomicalsXml_StarHasNoParent()
    {
        var system = CreateTestSystem();
        var doc = XmlExporter.Export(system);

        var star = doc.Root!.Elements("StellarBody").First();
        Assert.Equal(system.Star.Id, star.Attribute("Id")!.Value);
        Assert.Null(star.Attribute("Parent"));
    }

    [Fact]
    public void AstronomicalsXml_PlanetsHaveParent()
    {
        var system = CreateTestSystem();
        var doc = XmlExporter.Export(system);

        var bodies = doc.Root!.Elements()
            .Where(e => e.Name.LocalName is "TerrestrialBody" or "AtmosphericBody")
            .ToList();

        Assert.True(bodies.Count > 0);

        foreach (var body in bodies)
        {
            Assert.NotNull(body.Attribute("Parent")?.Value);
        }
    }

    [Fact]
    public void AstronomicalsXml_BodiesHaveRequiredElements()
    {
        var system = CreateTestSystem();
        var doc = XmlExporter.Export(system);

        foreach (var body in doc.Root!.Elements())
        {
            if (body.Name.LocalName == "StellarBody") continue;

            Assert.NotNull(body.Element("Orbit"));
            Assert.NotNull(body.Element("Rotation"));
            Assert.NotNull(body.Element("MeanRadius"));
            Assert.NotNull(body.Element("Mass"));
            Assert.NotNull(body.Element("Diffuse"));
        }
    }

    [Fact]
    public void SystemXml_HasCorrectStructure()
    {
        var system = CreateTestSystem();
        var doc = SystemXmlExporter.Export(system);

        Assert.Equal("System", doc.Root!.Name.LocalName);
        Assert.Equal(system.SystemId, doc.Root.Attribute("Id")!.Value);

        var loads = doc.Root.Elements("LoadFromLibrary").ToList();

        // First entry is the star with no Parent
        Assert.Equal(system.Star.Id, loads[0].Attribute("Id")!.Value);
        Assert.Null(loads[0].Attribute("Parent"));

        // All others have Parent
        for (int i = 1; i < loads.Count; i++)
        {
            Assert.NotNull(loads[i].Attribute("Parent")?.Value);
        }
    }

    [Fact]
    public void SystemXml_ContainsAllBodies()
    {
        var system = CreateTestSystem();
        var doc = SystemXmlExporter.Export(system);

        var loads = doc.Root!.Elements("LoadFromLibrary").ToList();

        int expectedCount = 1 + system.Planets.Count + system.Planets.Sum(p => p.Moons.Count);
        Assert.Equal(expectedCount, loads.Count);
    }

    [Fact]
    public void AstronomicalsXml_AtmosphericBodyHasAtmosphere()
    {
        var system = CreateTestSystem();
        var doc = XmlExporter.Export(system);

        var atmoBody = doc.Root!.Elements("AtmosphericBody").FirstOrDefault();
        if (atmoBody != null)
        {
            var atmosphere = atmoBody.Element("Atmosphere");
            Assert.NotNull(atmosphere);
            Assert.NotNull(atmosphere!.Element("Visual"));
            Assert.NotNull(atmosphere.Element("Physical"));
        }
    }
}
