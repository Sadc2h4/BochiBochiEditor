using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace BochiBochiEditor;

internal sealed class CfruTilesetSource
{
    public int Format { get; set; } = 1;
    public int HeaderAddress { get; set; }
    public bool Secondary { get; set; }
    public bool New { get; set; }
    public byte[] Header { get; set; } = Array.Empty<byte>();
    public int TileCount { get; set; }
    public int BlockCount { get; set; }
    public int BlockBytes { get; set; }
    public int BehaviorBytes { get; set; }
    public bool Compressed { get; set; }
    public string ImageSha256 { get; set; } = "";
    public byte[] Palettes { get; set; } = Array.Empty<byte>();
    public byte[] Blocks { get; set; } = Array.Empty<byte>();
    public byte[] Behaviors { get; set; } = Array.Empty<byte>();
}

internal static class CfruTilesetCodec
{
    public static string Folder(string project, int headerAddress, bool secondary) =>
        Path.Combine(project, "maps", "tilesets", (secondary ? "secondary_" : "primary_") + headerAddress.ToString("X6"));

    public static string Serialize(CfruTilesetSource source) =>
        JsonSerializer.Serialize(source, new JsonSerializerOptions { WriteIndented = true }) + Environment.NewLine;

    public static CfruTilesetSource Read(string folder)
    {
        var source = JsonSerializer.Deserialize<CfruTilesetSource>(File.ReadAllText(Path.Combine(folder, "tileset.json")))
            ?? throw new InvalidDataException("タイルセットのソースが空です。");
        if (source.Format != 1 || source.Header?.Length != 24 || source.Palettes?.Length != 512 ||
            source.Blocks?.Length != source.BlockCount * source.BlockBytes ||
            source.Behaviors?.Length != source.BlockCount * source.BehaviorBytes ||
            source.TileCount < 1 || source.TileCount > 1024 ||
            !source.New && source.HeaderAddress >= 0x1000000)
            throw new InvalidDataException("タイルセットのソース形式が不正です: " + folder);
        return source;
    }

    public static int Apply(byte[] rom, int allocationStart, string projectFolder,
        IDictionary<uint, uint> relocations)
    {
        string root = Path.Combine(projectFolder, "maps", "tilesets");
        if (!Directory.Exists(root)) return allocationStart;
        int next = allocationStart;
        foreach (string folder in Directory.EnumerateDirectories(root).OrderBy(p => p, StringComparer.OrdinalIgnoreCase))
        {
            if (!File.Exists(Path.Combine(folder, "tileset.json"))) continue;
            CfruTilesetSource source = Read(folder);
            string imagePath = Path.Combine(folder, "tiles.png");
            using Bitmap bitmap = new Bitmap(imagePath);
            if (bitmap.PixelFormat != PixelFormat.Format4bppIndexed || bitmap.Width % 8 != 0 || bitmap.Height % 8 != 0)
                throw new InvalidDataException("tiles.png は 4bpp・8 ピクセル単位で保存してください: " + imagePath);
            byte[] image = ImageProcessor.ImportSpriteFrom4bppPng(bitmap);
            if (image.Length != source.TileCount * 32)
                throw new InvalidDataException("tiles.png と tileset.json が一致しません: " + imagePath);
            byte[] storedImage = source.Compressed ? ImageProcessor.LZ77Comp(image, false) : image;
            byte[] header = (byte[])source.Header.Clone();
            WritePointer(header, 4, Put(rom, ref next, storedImage));
            WritePointer(header, 8, Put(rom, ref next, source.Palettes));
            WritePointer(header, 12, Put(rom, ref next, source.Blocks));
            WritePointer(header, 20, Put(rom, ref next, source.Behaviors));
            if (source.HeaderAddress < 0 || source.HeaderAddress + 24 > rom.Length)
                throw new InvalidDataException("タイルセット見出しが ROM の範囲外です。");
            int target = source.New ? Put(rom, ref next, header) : source.HeaderAddress;
            if (!source.New) Buffer.BlockCopy(header, 0, rom, target, 24);
            relocations[0x08000000u + (uint)source.HeaderAddress] = 0x08000000u + (uint)target;
        }
        return next;
    }

    public static void SaveImage(string path, byte[] image, byte[] palettes, int paletteNumber, int tileCount)
    {
        ImageProcessor.ExportSpriteTo4bppPng(path, image, MapTilePackage.ToColors(palettes, paletteNumber),
            128, tileCount / 16 * 8);
    }

    public static string Sha256(byte[] bytes) => Convert.ToHexString(SHA256.HashData(bytes));

    private static int Put(byte[] rom, ref int next, byte[] bytes)
    {
        if (bytes.Length == 0) return 0;
        next = (next + 3) & ~3;
        if ((long)next + bytes.Length > rom.Length)
            throw new InvalidDataException("タイルセットの配置先が ROM の範囲外です。");
        for (int i = 0; i < bytes.Length; i++)
            if (rom[next + i] != 0xFF) throw new InvalidDataException("タイルセットの配置先に既存データがあります。");
        int at = next;
        Buffer.BlockCopy(bytes, 0, rom, at, bytes.Length);
        next += bytes.Length;
        return at;
    }

    private static void WritePointer(byte[] bytes, int at, int target) =>
        Buffer.BlockCopy(BitConverter.GetBytes(target == 0 ? 0u : 0x08000000u + (uint)target), 0, bytes, at, 4);
}
