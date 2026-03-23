using StellarForge.Generation.Models;

namespace StellarForge.Runtime;

public static class RuntimeInjector
{
    /// <summary>
    /// Attempts to inject generated celestial bodies into the live CelestialSystem.
    /// Returns true if injection succeeded, false if fallback to XML is needed.
    /// </summary>
    public static bool TryInject(SystemData system)
    {
        try
        {
            // Attempt to access the game's CelestialSystem via reflection
            // This is a best-effort approach since we don't have the actual KSA.dll
            var ksaAssembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == "KSA");

            if (ksaAssembly == null)
                return false;

            var universeType = ksaAssembly.GetType("KSA.Universe");
            if (universeType == null)
                return false;

            var csProp = universeType.GetProperty("CelestialSystem",
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
            if (csProp == null)
                return false;

            var celestialSystem = csProp.GetValue(null);
            if (celestialSystem == null)
                return false;

            // Look for Astronomical constructor and template type
            var astroType = ksaAssembly.GetType("KSA.Astronomical");
            var templateType = ksaAssembly.GetType("KSA.AstronomicalTemplate");
            if (astroType == null || templateType == null)
                return false;

            // Attempt to construct bodies
            // This will likely need refinement once we can test against real KSA
            var constructor = astroType.GetConstructor(new[]
            {
                celestialSystem.GetType(), templateType, typeof(string)
            });

            if (constructor == null)
                return false;

            // If we get here, the API exists - attempt to create bodies
            // Star first, then planets, then moons
            // TODO: Build AstronomicalTemplate instances from SystemData
            // This requires understanding the template structure from KSA.dll

            return false; // For now, always fall back until we can test with real game
        }
        catch
        {
            return false; // Any error = fall back to XML export
        }
    }
}
