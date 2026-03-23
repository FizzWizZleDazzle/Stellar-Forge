namespace StellarForge.Export;

public static class ModTomlWriter
{
    public static void Write(string modTomlPath, string astronomicalsFile, string systemFile)
    {
        var content = $"""
            name = "StellarForge"
            assets = ["{astronomicalsFile}"]
            systems = ["{systemFile}"]
            """;

        Directory.CreateDirectory(Path.GetDirectoryName(modTomlPath)!);
        File.WriteAllText(modTomlPath, content);
    }
}
