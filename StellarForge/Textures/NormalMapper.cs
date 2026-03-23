namespace StellarForge.Textures;

public static class NormalMapper
{
    /// <summary>Generate tangent-space normal map from height map using Sobel filter</summary>
    public static byte[] GenerateFromHeightMap(float[] heightMap, int width, int height, float strength = 2.0f)
    {
        var normals = new byte[width * height * 3];

        Parallel.For(0, height, y =>
        {
            for (int x = 0; x < width; x++)
            {
                // Sobel filter with wrapping X, clamping Y
                float tl = Sample(heightMap, width, height, x - 1, y - 1);
                float t  = Sample(heightMap, width, height, x,     y - 1);
                float tr = Sample(heightMap, width, height, x + 1, y - 1);
                float l  = Sample(heightMap, width, height, x - 1, y);
                float r  = Sample(heightMap, width, height, x + 1, y);
                float bl = Sample(heightMap, width, height, x - 1, y + 1);
                float b  = Sample(heightMap, width, height, x,     y + 1);
                float br = Sample(heightMap, width, height, x + 1, y + 1);

                // Sobel operators
                float dx = (tr + 2 * r + br) - (tl + 2 * l + bl);
                float dy = (bl + 2 * b + br) - (tl + 2 * t + tr);

                // Normal vector
                float nx = -dx * strength;
                float ny = -dy * strength;
                float nz = 1.0f;

                // Normalize
                float len = MathF.Sqrt(nx * nx + ny * ny + nz * nz);
                nx /= len;
                ny /= len;
                nz /= len;

                // Encode: (n * 0.5 + 0.5) * 255
                int idx = (y * width + x) * 3;
                normals[idx]     = (byte)((nx * 0.5f + 0.5f) * 255);
                normals[idx + 1] = (byte)((ny * 0.5f + 0.5f) * 255);
                normals[idx + 2] = (byte)((nz * 0.5f + 0.5f) * 255);
            }
        });

        return normals;
    }

    private static float Sample(float[] map, int width, int height, int x, int y)
    {
        // Wrap X (spherical), clamp Y
        x = ((x % width) + width) % width;
        y = Math.Clamp(y, 0, height - 1);
        return map[y * width + x];
    }
}
