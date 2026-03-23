namespace StellarForge.Generation;

public static class NameGenerator
{
    private static readonly string[] Prefixes = { "Al", "Be", "Ca", "De", "El", "Fa", "Ga", "Ha", "In", "Ja", "Ka", "Le", "Ma", "Na", "Or", "Pa", "Qu", "Ra", "Sa", "Ta", "Ul", "Va", "Wa", "Xe", "Ya", "Za" };
    private static readonly string[] Middles = { "ra", "le", "ti", "no", "si", "me", "da", "ri", "ko", "pha", "the", "lo", "ni", "ga", "vi", "se", "to", "lu", "mi", "ne" };
    private static readonly string[] Suffixes = { "x", "s", "n", "r", "a", "us", "is", "on", "ar", "ia", "um", "os", "el", "an", "ix" };

    private static readonly string[] StarSuffixes = { " Prime", " Major", " Alpha", "", "", "", "" };

    public static string GenerateStarName(SeededRandom rng)
    {
        string name = rng.Pick(Prefixes) + rng.Pick(Middles) + rng.Pick(Suffixes);
        name += rng.Pick(StarSuffixes);
        return name;
    }

    public static string GeneratePlanetName(SeededRandom rng, string starName, int index)
    {
        // Use Roman-numeral style suffix or a generated name
        if (rng.Chance(0.5))
        {
            return $"{starName} {ToRoman(index + 1)}";
        }
        return rng.Pick(Prefixes) + rng.Pick(Middles) + rng.Pick(Suffixes);
    }

    public static string GenerateMoonName(SeededRandom rng, string planetName, int index)
    {
        if (rng.Chance(0.5))
        {
            return $"{planetName}-{(char)('a' + index)}";
        }
        return rng.Pick(Prefixes) + rng.Pick(Middles) + rng.Pick(Suffixes);
    }

    public static string GenerateSystemName(SeededRandom rng)
    {
        return rng.Pick(Prefixes) + rng.Pick(Middles) + rng.Pick(Suffixes) + " System";
    }

    private static string ToRoman(int number) => number switch
    {
        1 => "I", 2 => "II", 3 => "III", 4 => "IV", 5 => "V",
        6 => "VI", 7 => "VII", 8 => "VIII", 9 => "IX", 10 => "X",
        11 => "XI", 12 => "XII",
        _ => number.ToString()
    };
}
