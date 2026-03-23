using StellarForge.Generation.Models;

namespace StellarForge.Textures;

public static class ColorMapper
{
    private record GradientStop(float Position, byte R, byte G, byte B);

    private static readonly GradientStop[] RockyGradient =
    {
        new(0.0f, 60, 50, 40),
        new(0.3f, 100, 85, 65),
        new(0.5f, 140, 120, 95),
        new(0.7f, 170, 150, 120),
        new(0.9f, 200, 185, 160),
        new(1.0f, 230, 220, 210),
    };

    private static readonly GradientStop[] OceanWorldGradient =
    {
        new(0.0f, 10, 30, 80),     // deep ocean
        new(0.3f, 20, 60, 140),    // shallow ocean
        new(0.45f, 60, 120, 180),  // coast
        new(0.5f, 194, 178, 128),  // beach
        new(0.55f, 40, 120, 40),   // lowland
        new(0.7f, 80, 100, 40),    // highland
        new(0.85f, 140, 120, 90),  // mountain
        new(1.0f, 240, 240, 250),  // snow cap
    };

    private static readonly GradientStop[] GasGiantGradient =
    {
        new(0.0f, 180, 120, 60),
        new(0.15f, 220, 180, 120),
        new(0.3f, 200, 150, 80),
        new(0.45f, 240, 220, 180),
        new(0.6f, 180, 100, 50),
        new(0.75f, 210, 170, 110),
        new(0.9f, 190, 140, 70),
        new(1.0f, 220, 190, 140),
    };

    private static readonly GradientStop[] IceGiantGradient =
    {
        new(0.0f, 30, 80, 120),
        new(0.2f, 50, 120, 160),
        new(0.4f, 70, 140, 180),
        new(0.6f, 100, 170, 200),
        new(0.8f, 80, 150, 190),
        new(1.0f, 60, 110, 150),
    };

    private static readonly GradientStop[] DwarfGradient =
    {
        new(0.0f, 80, 80, 80),
        new(0.3f, 110, 105, 100),
        new(0.6f, 140, 135, 130),
        new(0.8f, 170, 165, 160),
        new(1.0f, 200, 195, 190),
    };

    private static readonly GradientStop[] StarGradient =
    {
        new(0.0f, 255, 200, 100),
        new(0.3f, 255, 230, 160),
        new(0.5f, 255, 255, 200),
        new(0.7f, 255, 240, 180),
        new(1.0f, 255, 210, 120),
    };

    private static readonly GradientStop[] MoonGradient =
    {
        new(0.0f, 70, 70, 70),
        new(0.3f, 100, 95, 90),
        new(0.5f, 130, 125, 120),
        new(0.7f, 160, 155, 150),
        new(1.0f, 190, 185, 180),
    };

    public static byte[] MapToRgb(float[] heightMap, int width, int height, PlanetType type)
    {
        var gradient = GetGradient(type);
        var rgb = new byte[width * height * 3];

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                float h = Math.Clamp(heightMap[idx], 0, 1);
                var (r, g, b) = SampleGradient(gradient, h);
                rgb[idx * 3] = r;
                rgb[idx * 3 + 1] = g;
                rgb[idx * 3 + 2] = b;
            }
        });

        return rgb;
    }

    public static byte[] MapStarToRgb(float[] heightMap, int width, int height, float colorR, float colorG, float colorB)
    {
        var rgb = new byte[width * height * 3];

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                float h = Math.Clamp(heightMap[idx], 0, 1);
                var (sr, sg, sb) = SampleGradient(StarGradient, h);
                rgb[idx * 3] = (byte)(sr * colorR);
                rgb[idx * 3 + 1] = (byte)(sg * colorG);
                rgb[idx * 3 + 2] = (byte)(sb * colorB);
            }
        });

        return rgb;
    }

    public static byte[] MapMoonToRgb(float[] heightMap, int width, int height)
    {
        return MapToRgbInternal(heightMap, width, height, MoonGradient);
    }

    private static byte[] MapToRgbInternal(float[] heightMap, int width, int height, GradientStop[] gradient)
    {
        var rgb = new byte[width * height * 3];
        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                int idx = y * width + x;
                float h = Math.Clamp(heightMap[idx], 0, 1);
                var (r, g, b) = SampleGradient(gradient, h);
                rgb[idx * 3] = r;
                rgb[idx * 3 + 1] = g;
                rgb[idx * 3 + 2] = b;
            }
        });
        return rgb;
    }

    private static GradientStop[] GetGradient(PlanetType type) => type switch
    {
        PlanetType.Rocky => RockyGradient,
        PlanetType.OceanWorld => OceanWorldGradient,
        PlanetType.GasGiant => GasGiantGradient,
        PlanetType.IceGiant => IceGiantGradient,
        PlanetType.Dwarf => DwarfGradient,
        _ => RockyGradient,
    };

    private static (byte r, byte g, byte b) SampleGradient(GradientStop[] gradient, float t)
    {
        if (t <= gradient[0].Position) return (gradient[0].R, gradient[0].G, gradient[0].B);
        if (t >= gradient[^1].Position) return (gradient[^1].R, gradient[^1].G, gradient[^1].B);

        for (int i = 0; i < gradient.Length - 1; i++)
        {
            if (t >= gradient[i].Position && t <= gradient[i + 1].Position)
            {
                float localT = (t - gradient[i].Position) / (gradient[i + 1].Position - gradient[i].Position);
                byte r = (byte)(gradient[i].R + localT * (gradient[i + 1].R - gradient[i].R));
                byte g = (byte)(gradient[i].G + localT * (gradient[i + 1].G - gradient[i].G));
                byte b = (byte)(gradient[i].B + localT * (gradient[i + 1].B - gradient[i].B));
                return (r, g, b);
            }
        }

        return (gradient[^1].R, gradient[^1].G, gradient[^1].B);
    }
}
