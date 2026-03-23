using System.Xml.Linq;
using StellarForge.Generation.Models;

namespace StellarForge.Export;

public static class SystemXmlExporter
{
    public static XDocument Export(SystemData system)
    {
        var root = new XElement("System",
            new XAttribute("Id", system.SystemId),
            new XElement("DisplayName", new XAttribute("Value", system.DisplayName)));

        // Star first - no Parent
        root.Add(new XElement("LoadFromLibrary", new XAttribute("Id", system.Star.Id)));

        // Planets - Parent = star
        foreach (var planet in system.Planets)
        {
            root.Add(new XElement("LoadFromLibrary",
                new XAttribute("Id", planet.Id),
                new XAttribute("Parent", system.Star.Id)));

            // Moons - Parent = planet
            foreach (var moon in planet.Moons)
            {
                root.Add(new XElement("LoadFromLibrary",
                    new XAttribute("Id", moon.Id),
                    new XAttribute("Parent", planet.Id)));
            }
        }

        return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
    }

    public static void Save(SystemData system, string path)
    {
        var doc = Export(system);
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        doc.Save(path);
    }
}
