using StellarForge.Generation.Models;

namespace StellarForge.Generation;

public class SystemGenerator
{
    public delegate void ProgressCallback(float progress, string status);

    public static SystemData Generate(GeneratorConfig config, ProgressCallback? onProgress = null)
    {
        int seed = config.SeedText.GetHashCode();
        var rng = new SeededRandom(seed);

        onProgress?.Invoke(0.05f, "Generating star...");

        // Generate star
        string starName = NameGenerator.GenerateStarName(rng);
        string starId = SanitizeId(starName);
        var star = StarGenerator.Generate(rng, starId);
        star.Name = starName;

        onProgress?.Invoke(0.15f, "Generating planets...");

        // Generate planets
        var planets = PlanetGenerator.Generate(rng, star, config);

        onProgress?.Invoke(0.30f, "Naming planets...");

        // Assign names and IDs
        for (int i = 0; i < planets.Count; i++)
        {
            string name = NameGenerator.GeneratePlanetName(rng, starName, i);
            string id = SanitizeId(name);
            planets[i].Name = name;
            planets[i].Id = id;
            planets[i].ParentId = star.Id;

            // Update texture paths with proper IDs
            planets[i].DiffuseTexturePath = $"Textures/{id}_Diffuse.png";
            planets[i].NormalTexturePath = $"Textures/{id}_Normal.png";
            planets[i].HeightTexturePath = $"Textures/{id}_Height.png";
        }

        onProgress?.Invoke(0.40f, "Generating atmospheres...");

        // Generate atmospheres
        foreach (var planet in planets)
        {
            planet.Atmosphere = AtmosphereGenerator.Generate(rng, planet.PlanetType, planet.MeanRadiusKm);
        }

        onProgress?.Invoke(0.55f, "Generating moons...");

        // Generate moons
        foreach (var planet in planets)
        {
            MoonGenerator.GenerateMoons(rng, planet, star, config);
        }

        onProgress?.Invoke(0.70f, "Assigning textures...");

        // Update moon IDs/paths after name generation
        foreach (var planet in planets)
        {
            for (int j = 0; j < planet.Moons.Count; j++)
            {
                var moon = planet.Moons[j];
                string moonId = SanitizeId(moon.Name);
                moon.Id = moonId;
                moon.ParentId = planet.Id;
                moon.DiffuseTexturePath = $"Textures/{moonId}_Diffuse.png";
                moon.NormalTexturePath = $"Textures/{moonId}_Normal.png";
                moon.HeightTexturePath = $"Textures/{moonId}_Height.png";
            }
        }

        // System name
        string systemName = string.IsNullOrWhiteSpace(config.SystemName) || config.SystemName == "Generated System"
            ? NameGenerator.GenerateSystemName(rng)
            : config.SystemName;
        string systemId = SanitizeId(systemName);

        onProgress?.Invoke(1.0f, "Complete!");

        return new SystemData
        {
            SystemId = systemId,
            DisplayName = systemName,
            Seed = config.SeedText,
            Star = star,
            Planets = planets
        };
    }

    private static string SanitizeId(string name)
    {
        // KSA IDs: no dots, no backslashes, alphanumeric + underscores
        var chars = name.Where(c => char.IsLetterOrDigit(c) || c == '_' || c == ' ').ToArray();
        return new string(chars).Replace(' ', '_').Trim('_');
    }
}
