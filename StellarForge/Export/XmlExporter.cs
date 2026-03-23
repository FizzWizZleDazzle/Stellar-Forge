using System.Xml.Linq;
using StellarForge.Generation.Models;

namespace StellarForge.Export;

public static class XmlExporter
{
    public static XDocument Export(SystemData system)
    {
        var root = new XElement("Astronomicals");

        // Star
        root.Add(BuildStellarBody(system.Star));

        // Planets and their moons
        foreach (var planet in system.Planets)
        {
            if (planet.HasAtmosphere || planet.PlanetType == PlanetType.GasGiant || planet.PlanetType == PlanetType.IceGiant)
                root.Add(BuildAtmosphericBody(planet));
            else
                root.Add(BuildTerrestrialBody(planet));

            foreach (var moon in planet.Moons)
                root.Add(BuildTerrestrialMoon(moon));
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
    }

    public static void Save(SystemData system, string path)
    {
        var doc = Export(system);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        doc.Save(path);
    }

    private static XElement BuildStellarBody(StarData star)
    {
        return new XElement("StellarBody",
            new XAttribute("Id", star.Id),
            new XElement("MeanRadius", new XAttribute("Km", star.MeanRadiusKm.ToString("G"))),
            new XElement("Mass", new XAttribute("Kg", star.MassKg.ToString("E3"))),
            new XElement("Diffuse",
                new XAttribute("Path", star.DiffuseTexturePath),
                new XAttribute("Category", "Terrain")),
            new XElement("Color",
                new XAttribute("R", star.ColorR.ToString("F2")),
                new XAttribute("G", star.ColorG.ToString("F2")),
                new XAttribute("B", star.ColorB.ToString("F2")))
        );
    }

    private static XElement BuildTerrestrialBody(PlanetData planet)
    {
        var elem = new XElement("TerrestrialBody",
            new XAttribute("Id", planet.Id),
            new XAttribute("Parent", planet.ParentId));

        elem.Add(new XElement("MeshCollection", new XAttribute("Id", planet.MeshCollectionId)));
        elem.Add(BuildOrbit(planet));
        elem.Add(BuildRotation(planet.IsTidallyLocked, planet.SiderealPeriodHours, planet.TiltDeg, planet.AzimuthDeg));
        elem.Add(new XElement("MeanRadius", new XAttribute("Km", planet.MeanRadiusKm.ToString("G"))));
        elem.Add(new XElement("Mass", new XAttribute("Kg", planet.MassKg.ToString("E3"))));
        elem.Add(new XElement("Color",
            new XAttribute("R", planet.ColorR.ToString("F2")),
            new XAttribute("G", planet.ColorG.ToString("F2")),
            new XAttribute("B", planet.ColorB.ToString("F2"))));
        elem.Add(new XElement("Diffuse",
            new XAttribute("Path", planet.DiffuseTexturePath),
            new XAttribute("Category", "Terrain")));
        elem.Add(new XElement("Normal",
            new XAttribute("Path", planet.NormalTexturePath),
            new XAttribute("Category", "Terrain")));
        elem.Add(BuildHeight(planet.HeightTexturePath, planet.HeightMinKm, planet.HeightMaxKm));

        return elem;
    }

    private static XElement BuildAtmosphericBody(PlanetData planet)
    {
        var elem = new XElement("AtmosphericBody",
            new XAttribute("Id", planet.Id),
            new XAttribute("Parent", planet.ParentId));

        elem.Add(new XElement("MeshCollection", new XAttribute("Id", planet.MeshCollectionId)));
        elem.Add(BuildOrbit(planet));
        elem.Add(BuildRotation(planet.IsTidallyLocked, planet.SiderealPeriodHours, planet.TiltDeg, planet.AzimuthDeg));
        elem.Add(new XElement("MeanRadius", new XAttribute("Km", planet.MeanRadiusKm.ToString("G"))));
        elem.Add(new XElement("Mass", new XAttribute("Kg", planet.MassKg.ToString("E3"))));
        if (planet.MeanAnomalyAtEpochDeg != 0)
            elem.Add(new XElement("MeanAnomalyAtEpoch", new XAttribute("Degrees", planet.MeanAnomalyAtEpochDeg.ToString("F2"))));
        elem.Add(new XElement("Diffuse",
            new XAttribute("Path", planet.DiffuseTexturePath),
            new XAttribute("Category", "Terrain")));
        elem.Add(new XElement("Normal",
            new XAttribute("Path", planet.NormalTexturePath),
            new XAttribute("Category", "Terrain")));
        elem.Add(BuildHeight(planet.HeightTexturePath, planet.HeightMinKm, planet.HeightMaxKm));
        elem.Add(new XElement("Color",
            new XAttribute("R", planet.ColorR.ToString("F2")),
            new XAttribute("G", planet.ColorG.ToString("F2")),
            new XAttribute("B", planet.ColorB.ToString("F2"))));

        if (planet.Atmosphere != null)
            elem.Add(BuildAtmosphere(planet.Atmosphere));

        return elem;
    }

    private static XElement BuildTerrestrialMoon(MoonData moon)
    {
        var elem = new XElement("TerrestrialBody",
            new XAttribute("Id", moon.Id),
            new XAttribute("Parent", moon.ParentId));

        elem.Add(new XElement("MeshCollection", new XAttribute("Id", "Asteroid")));
        elem.Add(BuildOrbit(moon.SemiMajorAxisKm, moon.InclinationDeg, moon.Eccentricity,
            moon.LongitudeOfAscendingNodeDeg, moon.ArgumentOfPeriapsisDeg, moon.TimeAtPeriapsisSeconds));
        elem.Add(BuildRotation(moon.IsTidallyLocked, moon.SiderealPeriodHours, moon.TiltDeg, 0));
        elem.Add(new XElement("MeanRadius", new XAttribute("Km", moon.MeanRadiusKm.ToString("G"))));
        elem.Add(new XElement("Mass", new XAttribute("Kg", moon.MassKg.ToString("E3"))));
        elem.Add(new XElement("Color",
            new XAttribute("R", moon.ColorR.ToString("F2")),
            new XAttribute("G", moon.ColorG.ToString("F2")),
            new XAttribute("B", moon.ColorB.ToString("F2"))));
        elem.Add(new XElement("Diffuse",
            new XAttribute("Path", moon.DiffuseTexturePath),
            new XAttribute("Category", "Terrain")));
        elem.Add(new XElement("Normal",
            new XAttribute("Path", moon.NormalTexturePath),
            new XAttribute("Category", "Terrain")));
        elem.Add(BuildHeight(moon.HeightTexturePath, moon.HeightMinKm, moon.HeightMaxKm));

        return elem;
    }

    private static XElement BuildOrbit(PlanetData p) =>
        BuildOrbit(p.SemiMajorAxisKm, p.InclinationDeg, p.Eccentricity,
            p.LongitudeOfAscendingNodeDeg, p.ArgumentOfPeriapsisDeg, p.TimeAtPeriapsisSeconds);

    private static XElement BuildOrbit(double smaKm, double incDeg, double ecc, double lanDeg, double aopDeg, double tapSec)
    {
        return new XElement("Orbit",
            new XAttribute("DefinitionFrame", "Ecliptic"),
            new XElement("SemiMajorAxis", new XAttribute("Km", smaKm.ToString("E15"))),
            new XElement("Inclination", new XAttribute("Degrees", incDeg.ToString("F2"))),
            new XElement("Eccentricity", new XAttribute("Value", ecc.ToString("F3"))),
            new XElement("LongitudeOfAscendingNode", new XAttribute("Degrees", lanDeg.ToString("F2"))),
            new XElement("ArgumentOfPeriapsis", new XAttribute("Degrees", aopDeg.ToString("F2"))),
            new XElement("TimeAtPeriapsis", new XAttribute("Seconds", tapSec.ToString("F2")))
        );
    }

    private static XElement BuildRotation(bool tidallyLocked, double siderealHours, double tiltDeg, double azimuthDeg)
    {
        var rot = new XElement("Rotation",
            new XAttribute("DefinitionFrame", "Ecliptic"));

        if (tidallyLocked)
        {
            rot.Add(new XElement("IsTidallyLocked", new XAttribute("Value", "true")));
        }
        else
        {
            rot.Add(new XElement("SiderealPeriod", new XAttribute("Hours", siderealHours.ToString("F4"))));
        }

        rot.Add(new XElement("Tilt", new XAttribute("Degrees", tiltDeg.ToString("F2"))));
        rot.Add(new XElement("Azimuth", new XAttribute("Degrees", azimuthDeg.ToString("F2"))));
        rot.Add(new XElement("InitialParentFacingLongitude", new XAttribute("Degrees", "0")));

        return rot;
    }

    private static XElement BuildHeight(string path, double minKm, double maxKm)
    {
        return new XElement("Height",
            new XAttribute("Path", path),
            new XAttribute("Category", "Terrain"),
            new XElement("Minimum", new XAttribute("Km", minKm.ToString("F4"))),
            new XElement("Maximum", new XAttribute("Km", maxKm.ToString("F4")))
        );
    }

    private static XElement BuildAtmosphere(AtmosphereData atmo)
    {
        var visual = new XElement("Visual",
            new XElement("RayleighScattering",
                new XElement("Coefficients",
                    new XAttribute("R", atmo.RayleighR.ToString("F4")),
                    new XAttribute("G", atmo.RayleighG.ToString("F4")),
                    new XAttribute("B", atmo.RayleighB.ToString("F4"))),
                new XElement("ScaleHeight", new XAttribute("Km", atmo.RayleighScaleHeightKm.ToString("F1")))),
            new XElement("MieScattering",
                new XElement("Coefficients",
                    new XAttribute("R", atmo.MieR.ToString("F5")),
                    new XAttribute("G", atmo.MieG.ToString("F5")),
                    new XAttribute("B", atmo.MieB.ToString("F5"))),
                new XElement("ScaleHeight", new XAttribute("Km", atmo.MieScaleHeightKm.ToString("F1"))),
                new XElement("PhaseFunctionAsymmetry",
                    new XAttribute("X", atmo.MiePhaseAsymmetry.ToString("F2")),
                    new XAttribute("Y", atmo.MiePhaseAsymmetry.ToString("F2")),
                    new XAttribute("Z", atmo.MiePhaseAsymmetry.ToString("F2"))),
                new XElement("AbsorptionMultiplier", new XAttribute("Value", atmo.MieAbsorptionMultiplier.ToString("F4")))));

        // Add ozone if present
        if (atmo.OzoneR != 0 || atmo.OzoneG != 0 || atmo.OzoneB != 0)
        {
            visual.Add(new XElement("Ozone",
                new XElement("Coefficients",
                    new XAttribute("R", atmo.OzoneR.ToString("G")),
                    new XAttribute("G", atmo.OzoneG.ToString("G")),
                    new XAttribute("B", atmo.OzoneB.ToString("G"))),
                new XElement("Altitude", new XAttribute("Km", atmo.OzoneAltitudeKm.ToString("F0"))),
                new XElement("Extent", new XAttribute("Km", atmo.OzoneExtentKm.ToString("F0")))));
        }

        visual.Add(new XElement("StartHeight", new XAttribute("Km", atmo.StartHeightKm.ToString("F0"))));

        var physical = new XElement("Physical",
            new XElement("SeaLevelPressure", new XAttribute("Atm", atmo.SeaLevelPressureAtm.ToString("F2"))),
            new XElement("SeaLevelDensity", new XAttribute("KgPerM3", atmo.SeaLevelDensityKgPerM3.ToString("F3"))),
            new XElement("ScaleHeight", new XAttribute("Km", atmo.PhysicalScaleHeightKm.ToString("F1"))));

        return new XElement("Atmosphere", visual, physical);
    }
}
