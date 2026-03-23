using System.IO.Compression;

namespace StellarForge.Textures;

/// <summary>Minimal PNG encoder - RGB 8-bit, no external dependencies</summary>
public static class PngWriter
{
    private static readonly byte[] Signature = { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    public static void WriteRgb(string path, byte[] pixels, int width, int height)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);

        bw.Write(Signature);
        WriteIhdr(bw, width, height, 8, 2); // 8-bit RGB
        WriteIdat(bw, pixels, width, height, 3);
        WriteIend(bw);
    }

    public static void WriteGrayscale16(string path, ushort[] pixels, int width, int height)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);

        bw.Write(Signature);
        WriteIhdr(bw, width, height, 16, 0); // 16-bit grayscale

        // Convert ushort to big-endian bytes
        byte[] raw = new byte[width * height * 2];
        for (int i = 0; i < pixels.Length; i++)
        {
            raw[i * 2] = (byte)(pixels[i] >> 8);
            raw[i * 2 + 1] = (byte)(pixels[i] & 0xFF);
        }
        WriteIdat(bw, raw, width, height, 2);
        WriteIend(bw);
    }

    private static void WriteIhdr(BinaryWriter bw, int width, int height, byte bitDepth, byte colorType)
    {
        using var ms = new MemoryStream();
        using var w = new BinaryWriter(ms);
        w.Write(ToBigEndian(width));
        w.Write(ToBigEndian(height));
        w.Write(bitDepth);
        w.Write(colorType);
        w.Write((byte)0); // compression
        w.Write((byte)0); // filter
        w.Write((byte)0); // interlace
        WriteChunk(bw, "IHDR", ms.ToArray());
    }

    private static void WriteIdat(BinaryWriter bw, byte[] pixels, int width, int height, int bytesPerPixel)
    {
        int stride = width * bytesPerPixel;

        // Build filtered scanlines (filter type 0 = None for each row)
        using var raw = new MemoryStream();
        for (int y = 0; y < height; y++)
        {
            raw.WriteByte(0); // filter type: None
            raw.Write(pixels, y * stride, stride);
        }

        // Deflate compress
        using var compressed = new MemoryStream();
        // zlib header
        compressed.WriteByte(0x78);
        compressed.WriteByte(0x01);
        using (var deflate = new DeflateStream(compressed, CompressionLevel.Fastest, leaveOpen: true))
        {
            deflate.Write(raw.ToArray());
        }

        // Adler32 checksum
        var rawBytes = raw.ToArray();
        uint adler = Adler32(rawBytes);
        compressed.WriteByte((byte)(adler >> 24));
        compressed.WriteByte((byte)(adler >> 16));
        compressed.WriteByte((byte)(adler >> 8));
        compressed.WriteByte((byte)(adler));

        WriteChunk(bw, "IDAT", compressed.ToArray());
    }

    private static void WriteIend(BinaryWriter bw) => WriteChunk(bw, "IEND", Array.Empty<byte>());

    private static void WriteChunk(BinaryWriter bw, string type, byte[] data)
    {
        byte[] typeBytes = System.Text.Encoding.ASCII.GetBytes(type);
        bw.Write(ToBigEndian(data.Length));
        bw.Write(typeBytes);
        bw.Write(data);

        // CRC32 over type + data
        uint crc = Crc32(typeBytes, data);
        bw.Write(ToBigEndian((int)crc));
    }

    private static byte[] ToBigEndian(int value) => new[]
    {
        (byte)(value >> 24), (byte)(value >> 16), (byte)(value >> 8), (byte)value
    };

    private static uint Crc32(byte[] type, byte[] data)
    {
        uint crc = 0xFFFFFFFF;
        foreach (byte b in type) crc = CrcUpdate(crc, b);
        foreach (byte b in data) crc = CrcUpdate(crc, b);
        return crc ^ 0xFFFFFFFF;
    }

    private static uint CrcUpdate(uint crc, byte b)
    {
        crc ^= b;
        for (int i = 0; i < 8; i++)
            crc = (crc & 1) != 0 ? (crc >> 1) ^ 0xEDB88320 : crc >> 1;
        return crc;
    }

    private static uint Adler32(byte[] data)
    {
        uint a = 1, b = 0;
        foreach (byte d in data)
        {
            a = (a + d) % 65521;
            b = (b + a) % 65521;
        }
        return (b << 16) | a;
    }
}
