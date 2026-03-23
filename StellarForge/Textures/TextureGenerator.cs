using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;
using ILGPU.Runtime.OpenCL;
using StellarForge.Generation.Models;

namespace StellarForge.Textures;

public class TextureGenerator : IDisposable
{
    private Context? _context;
    private Accelerator? _accelerator;
    private bool _useGpu;

    public void Initialize(bool useGpu)
    {
        _useGpu = useGpu;
        if (!useGpu) return;

        try
        {
            _context = Context.Create(b => b.Default().EnableAlgorithms());

            // Try CUDA first, then OpenCL
            var device = _context.GetPreferredDevice(false);
            if (device.AcceleratorType == AcceleratorType.CPU)
            {
                // No GPU found, fallback to CPU
                _useGpu = false;
                _context.Dispose();
                _context = null;
                return;
            }

            _accelerator = device.CreateAccelerator(_context);
        }
        catch
        {
            _useGpu = false;
            _context?.Dispose();
            _context = null;
            _accelerator = null;
        }
    }

    public delegate void ProgressCallback(float progress, string status);

    public void GenerateAllTextures(SystemData system, string outputPath, int resolution, ProgressCallback? onProgress = null)
    {
        int seed = system.Seed.GetHashCode();
        var rng = new Random(seed);
        Directory.CreateDirectory(outputPath);

        int totalBodies = 1 + system.Planets.Count + system.Planets.Sum(p => p.Moons.Count);
        int current = 0;

        // Star texture
        onProgress?.Invoke((float)current / totalBodies, $"Generating star texture: {system.Star.Name}");
        GenerateStarTexture(system.Star, outputPath, resolution, rng.Next());
        current++;

        // Planet textures
        foreach (var planet in system.Planets)
        {
            onProgress?.Invoke((float)current / totalBodies, $"Generating planet texture: {planet.Name}");
            GeneratePlanetTexture(planet, outputPath, resolution, rng.Next());
            current++;

            // Moon textures
            foreach (var moon in planet.Moons)
            {
                onProgress?.Invoke((float)current / totalBodies, $"Generating moon texture: {moon.Name}");
                GenerateMoonTexture(moon, outputPath, resolution / 2, rng.Next()); // half res for moons
                current++;
            }
        }

        onProgress?.Invoke(1.0f, "All textures generated");
    }

    private void GenerateStarTexture(StarData star, string outputPath, int resolution, int seed)
    {
        int w = resolution, h = resolution / 2;
        var heightMap = GenerateNoiseMap(w, h, seed, octaves: 4, scale: 5.0f);
        var diffuse = ColorMapper.MapStarToRgb(heightMap, w, h, star.ColorR, star.ColorG, star.ColorB);
        PngWriter.WriteRgb(Path.Combine(outputPath, star.DiffuseTexturePath), diffuse, w, h);
    }

    private void GeneratePlanetTexture(PlanetData planet, string outputPath, int resolution, int seed)
    {
        int w = resolution, h = resolution / 2;

        float yStretch = (planet.PlanetType == PlanetType.GasGiant || planet.PlanetType == PlanetType.IceGiant)
            ? 4.0f  // banded appearance for gas/ice giants
            : 1.0f;

        int octaves = planet.PlanetType switch
        {
            PlanetType.GasGiant => 4,
            PlanetType.IceGiant => 5,
            _ => 6
        };

        var heightMap = GenerateNoiseMap(w, h, seed, octaves: octaves, scale: 4.0f, yStretch: yStretch);

        // Diffuse
        var diffuse = ColorMapper.MapToRgb(heightMap, w, h, planet.PlanetType);
        PngWriter.WriteRgb(Path.Combine(outputPath, planet.DiffuseTexturePath), diffuse, w, h);

        // Normal (not for gas/ice giants)
        if (planet.PlanetType != PlanetType.GasGiant && planet.PlanetType != PlanetType.IceGiant)
        {
            var normals = NormalMapper.GenerateFromHeightMap(heightMap, w, h);
            PngWriter.WriteRgb(Path.Combine(outputPath, planet.NormalTexturePath), normals, w, h);

            // Height as 16-bit grayscale
            var height16 = new ushort[w * h];
            for (int i = 0; i < heightMap.Length; i++)
                height16[i] = (ushort)(Math.Clamp(heightMap[i], 0, 1) * 65535);
            PngWriter.WriteGrayscale16(Path.Combine(outputPath, planet.HeightTexturePath), height16, w, h);
        }
    }

    private void GenerateMoonTexture(MoonData moon, string outputPath, int resolution, int seed)
    {
        int w = resolution, h = resolution / 2;
        var heightMap = GenerateNoiseMap(w, h, seed, octaves: 5, scale: 5.0f);

        var diffuse = ColorMapper.MapMoonToRgb(heightMap, w, h);
        PngWriter.WriteRgb(Path.Combine(outputPath, moon.DiffuseTexturePath), diffuse, w, h);

        var normals = NormalMapper.GenerateFromHeightMap(heightMap, w, h);
        PngWriter.WriteRgb(Path.Combine(outputPath, moon.NormalTexturePath), normals, w, h);

        var height16 = new ushort[w * h];
        for (int i = 0; i < heightMap.Length; i++)
            height16[i] = (ushort)(Math.Clamp(heightMap[i], 0, 1) * 65535);
        PngWriter.WriteGrayscale16(Path.Combine(outputPath, moon.HeightTexturePath), height16, w, h);
    }

    private float[] GenerateNoiseMap(int width, int height, int seed, int octaves = 6, float scale = 4.0f, float yStretch = 1.0f)
    {
        if (_useGpu && _accelerator != null)
        {
            return NoiseGenerator.GenerateSphericalGpu(_accelerator, width, height, seed,
                octaves: octaves, scale: scale, yStretch: yStretch);
        }
        return NoiseGenerator.GenerateSphericalCpu(width, height, seed,
            octaves: octaves, scale: scale, yStretch: yStretch);
    }

    public void Dispose()
    {
        _accelerator?.Dispose();
        _context?.Dispose();
    }
}
