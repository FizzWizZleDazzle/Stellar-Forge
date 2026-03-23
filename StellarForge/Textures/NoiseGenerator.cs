using ILGPU;
using ILGPU.Algorithms;
using ILGPU.Runtime;

namespace StellarForge.Textures;

public static class NoiseGenerator
{
    // Simplex noise constants - no lookup tables (ILGPU compatible)
    private static readonly int[] Perm;

    static NoiseGenerator()
    {
        // Generate a fixed permutation table
        Perm = new int[512];
        int[] p = new int[256];
        for (int i = 0; i < 256; i++) p[i] = i;
        // Fisher-Yates with fixed seed
        var rng = new Random(42);
        for (int i = 255; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (p[i], p[j]) = (p[j], p[i]);
        }
        for (int i = 0; i < 512; i++) Perm[i] = p[i & 255];
    }

    /// <summary>Generate a spherically-mapped height map using FBM noise (CPU)</summary>
    public static float[] GenerateSphericalCpu(int width, int height, int seed, int octaves = 6,
        float lacunarity = 2.0f, float persistence = 0.5f, float scale = 3.0f, float yStretch = 1.0f)
    {
        var result = new float[width * height];
        float seedOffset = seed * 100.0f;

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                // UV to sphere
                float u = (float)x / width;
                float v = (float)y / height;
                float theta = u * 2.0f * MathF.PI;
                float phi = v * MathF.PI;

                float nx = MathF.Sin(phi) * MathF.Cos(theta);
                float ny = MathF.Cos(phi);
                float nz = MathF.Sin(phi) * MathF.Sin(theta);

                // FBM
                float value = 0;
                float amplitude = 1.0f;
                float frequency = scale;
                float maxValue = 0;

                for (int o = 0; o < octaves; o++)
                {
                    float sx = nx * frequency + seedOffset;
                    float sy = ny * frequency * yStretch + seedOffset;
                    float sz = nz * frequency + seedOffset;
                    value += Noise3D(sx, sy, sz) * amplitude;
                    maxValue += amplitude;
                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                result[y * width + x] = (value / maxValue + 1.0f) * 0.5f; // normalize to [0,1]
            }
        });

        return result;
    }

    /// <summary>Generate spherical height map on GPU via ILGPU</summary>
    public static float[] GenerateSphericalGpu(Accelerator accelerator, int width, int height, int seed,
        int octaves = 6, float lacunarity = 2.0f, float persistence = 0.5f, float scale = 3.0f, float yStretch = 1.0f)
    {
        var permBuffer = accelerator.Allocate1D<int>(Perm);
        var output = accelerator.Allocate2DDenseY<float>(new Index2D(width, height));

        float seedOffset = seed * 100.0f;

        var kernel = accelerator.LoadAutoGroupedStreamKernel<
            Index2D, ArrayView1D<int, Stride1D.Dense>, ArrayView2D<float, Stride2D.DenseY>,
            int, int, int, float, float, float, float, float>(SphericalNoiseKernel);

        kernel(new Index2D(width, height), permBuffer.View, output.View,
            width, height, octaves, lacunarity, persistence, scale, yStretch, seedOffset);

        accelerator.Synchronize();

        var result = new float[width * height];
        var data = output.GetAsArray2D();
        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                result[y * width + x] = data[x, y];

        permBuffer.Dispose();
        output.Dispose();

        return result;
    }

    private static void SphericalNoiseKernel(
        Index2D index,
        ArrayView1D<int, Stride1D.Dense> perm,
        ArrayView2D<float, Stride2D.DenseY> output,
        int width, int height, int octaves,
        float lacunarity, float persistence, float scale, float yStretch, float seedOffset)
    {
        int x = index.X;
        int y = index.Y;

        float u = (float)x / width;
        float v = (float)y / height;
        float theta = u * 2.0f * XMath.PI;
        float phi = v * XMath.PI;

        float nx = XMath.Sin(phi) * XMath.Cos(theta);
        float ny = XMath.Cos(phi);
        float nz = XMath.Sin(phi) * XMath.Sin(theta);

        float value = 0;
        float amplitude = 1.0f;
        float frequency = scale;
        float maxValue = 0;

        for (int o = 0; o < octaves; o++)
        {
            float sx = nx * frequency + seedOffset;
            float sy = ny * frequency * yStretch + seedOffset;
            float sz = nz * frequency + seedOffset;
            value += GpuNoise3D(perm, sx, sy, sz) * amplitude;
            maxValue += amplitude;
            amplitude *= persistence;
            frequency *= lacunarity;
        }

        output[index] = (value / maxValue + 1.0f) * 0.5f;
    }

    // GPU-compatible 3D noise (value noise with smooth interpolation)
    private static float GpuNoise3D(ArrayView1D<int, Stride1D.Dense> perm, float x, float y, float z)
    {
        int xi = (int)XMath.Floor(x) & 255;
        int yi = (int)XMath.Floor(y) & 255;
        int zi = (int)XMath.Floor(z) & 255;

        float xf = x - XMath.Floor(x);
        float yf = y - XMath.Floor(y);
        float zf = z - XMath.Floor(z);

        float u = Fade(xf);
        float v = Fade(yf);
        float w = Fade(zf);

        int a = perm[xi] + yi;
        int aa = perm[a & 255] + zi;
        int ab = perm[(a + 1) & 255] + zi;
        int b = perm[(xi + 1) & 255] + yi;
        int ba = perm[b & 255] + zi;
        int bb = perm[(b + 1) & 255] + zi;

        return Lerp(w,
            Lerp(v,
                Lerp(u, GpuGrad(perm[aa & 255], xf, yf, zf), GpuGrad(perm[ba & 255], xf - 1, yf, zf)),
                Lerp(u, GpuGrad(perm[ab & 255], xf, yf - 1, zf), GpuGrad(perm[bb & 255], xf - 1, yf - 1, zf))),
            Lerp(v,
                Lerp(u, GpuGrad(perm[(aa + 1) & 255], xf, yf, zf - 1), GpuGrad(perm[(ba + 1) & 255], xf - 1, yf, zf - 1)),
                Lerp(u, GpuGrad(perm[(ab + 1) & 255], xf, yf - 1, zf - 1), GpuGrad(perm[(bb + 1) & 255], xf - 1, yf - 1, zf - 1))));
    }

    // CPU 3D Perlin noise
    public static float Noise3D(float x, float y, float z)
    {
        int xi = (int)MathF.Floor(x) & 255;
        int yi = (int)MathF.Floor(y) & 255;
        int zi = (int)MathF.Floor(z) & 255;

        float xf = x - MathF.Floor(x);
        float yf = y - MathF.Floor(y);
        float zf = z - MathF.Floor(z);

        float u = Fade(xf);
        float v = Fade(yf);
        float w = Fade(zf);

        int a = Perm[xi] + yi;
        int aa = Perm[a] + zi;
        int ab = Perm[a + 1] + zi;
        int b = Perm[xi + 1] + yi;
        int ba = Perm[b] + zi;
        int bb = Perm[b + 1] + zi;

        return Lerp(w,
            Lerp(v,
                Lerp(u, Grad(Perm[aa], xf, yf, zf), Grad(Perm[ba], xf - 1, yf, zf)),
                Lerp(u, Grad(Perm[ab], xf, yf - 1, zf), Grad(Perm[bb], xf - 1, yf - 1, zf))),
            Lerp(v,
                Lerp(u, Grad(Perm[aa + 1], xf, yf, zf - 1), Grad(Perm[ba + 1], xf - 1, yf, zf - 1)),
                Lerp(u, Grad(Perm[ab + 1], xf, yf - 1, zf - 1), Grad(Perm[bb + 1], xf - 1, yf - 1, zf - 1))));
    }

    private static float Fade(float t) => t * t * t * (t * (t * 6 - 15) + 10);
    private static float Lerp(float t, float a, float b) => a + t * (b - a);

    private static float Grad(int hash, float x, float y, float z)
    {
        int h = hash & 15;
        float u = h < 8 ? x : y;
        float v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }

    private static float GpuGrad(int hash, float x, float y, float z)
    {
        int h = hash & 15;
        float u = h < 8 ? x : y;
        float v = h < 4 ? y : (h == 12 || h == 14 ? x : z);
        return ((h & 1) == 0 ? u : -u) + ((h & 2) == 0 ? v : -v);
    }
}
