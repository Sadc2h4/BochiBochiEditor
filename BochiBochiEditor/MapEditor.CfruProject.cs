using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace BochiBochiEditor;

public partial class MapEditor
{
    private string cfruProjectFolder;
    private byte[] cfruBaseRom;
    private List<int> cfruBaseBankCounts;
    private Dictionary<uint, CfruItemBall> cfruManagedItems = new();
    private Dictionary<uint, uint> cfruTilesetIds = new();
    private Dictionary<string, string> cfruMapAssistHashes = new(StringComparer.OrdinalIgnoreCase);
    private bool cfruOpening;
    private string cfruOpeningFolder;

    public bool IsCfruProject => cfruProjectFolder != null;

    // The start page can call this after collecting the two paths. No UI control is
    // created here, so the existing ROM opening route remains independent.
    public void OpenCfruProject(string baseRomPath, string cfruFolder)
    {
        if (string.IsNullOrWhiteSpace(baseRomPath) || !File.Exists(baseRomPath))
            throw new FileNotFoundException("ベース ROM が見つかりません。", baseRomPath);
        if (string.IsNullOrWhiteSpace(cfruFolder) || !Directory.Exists(cfruFolder) ||
            !File.Exists(Path.Combine(cfruFolder, "scripts", "make.py")))
            throw new DirectoryNotFoundException("NeoDark-CFRU のフォルダを指定してください。");
        byte[] baseRom = File.ReadAllBytes(baseRomPath);
        if (baseRom.Length != 16 * 1024 * 1024 && baseRom.Length != 32 * 1024 * 1024)
            throw new InvalidDataException("16 MiB または 32 MiB の BPRJ ベース ROM を指定してください。");
        if (GameProfile.Detect(baseRom)?.Code != "BPRJ")
            throw new InvalidDataException("BPRJ ベース ROM を指定してください。");
        byte[] working = (byte[])baseRom.Clone();
        if (working.Length < 32 * 1024 * 1024)
        {
            Array.Resize(ref working, 32 * 1024 * 1024);
            working.AsSpan(baseRom.Length).Fill(0xFF);
        }
        string manifestPath = Path.Combine(cfruFolder, "maps", "project.json");
        CfruMapManifest manifest = null;
        var loadedItems = new Dictionary<uint, CfruItemBall>();
        var tilesetPointers = new Dictionary<uint, uint>();
        var maps = new List<CfruMapSource>();
        if (File.Exists(manifestPath))
        {
            manifest = JsonSerializer.Deserialize<CfruMapManifest>(File.ReadAllText(manifestPath));
            if (manifest?.Format != 1 || manifest.BaseBankCounts == null || manifest.BaseBankCounts.Count == 0)
                throw new InvalidDataException("maps/project.json の形式が不正です。");
            string mapRoot = Path.Combine(cfruFolder, "maps");
            var mapFiles = Directory.EnumerateFiles(mapRoot, "*.json", SearchOption.AllDirectories)
                .Where(p => int.TryParse(Path.GetFileName(Path.GetDirectoryName(p)), out _) &&
                    int.TryParse(Path.GetFileNameWithoutExtension(p), out _))
                .OrderBy(p => p, StringComparer.OrdinalIgnoreCase).ToList();
            maps = mapFiles.Select(CfruMapCodec.Read).ToList();
            if (maps.Count != maps.Select(m => (m.Bank, m.Number)).Distinct().Count())
                throw new InvalidDataException("同じバンク・マップ番号のソースが複数あります。");
            int bankTable = checked((int)(BitConverter.ToUInt32(working, 0x54B0C) - 0x08000000U));
            int next = CfruTilesetCodec.Apply(working, 0x1D00000, cfruFolder, tilesetPointers);
            CfruMapCodec.Apply(working, bankTable, manifest.BaseBankCounts, maps, next, loadedItems, tilesetPointers);
        }
        else if (Directory.Exists(Path.Combine(cfruFolder, "maps")) &&
            Directory.EnumerateFiles(Path.Combine(cfruFolder, "maps"), "*.json", SearchOption.AllDirectories).Any())
            throw new InvalidDataException("maps/project.json is required when map sources exist.");
        CfruWildCodec.ApplyPreview(working, maps);
        cfruOpening = true;
        cfruOpeningFolder = cfruFolder;
        try { ApplyLoadedRom(working, baseRomPath); }
        finally { cfruOpening = false; cfruOpeningFolder = null; }
        if (!ReferenceEquals(this.romData, working))
            throw new InvalidDataException("ベース ROM を読み込めませんでした。");
        cfruBaseRom = baseRom;
        cfruProjectFolder = Path.GetFullPath(cfruFolder);
        cfruBaseBankCounts = manifest?.BaseBankCounts ?? ReadBankCountsFromLoadedMaps();
        cfruManagedItems = loadedItems;
        cfruTilesetIds = tilesetPointers.ToDictionary(p => p.Value, p => p.Key);
        cfruMapAssistHashes = SnapshotMapAssist();
    }

    // Returns relative source paths for the start page's copyable save result.
    public IReadOnlyList<string> SaveCfruProject()
    {
        if (!IsCfruProject || romData == null || cfruBaseRom == null)
            throw new InvalidOperationException("CFRU プロジェクトが開かれていません。");
        if (!ConfirmSaveIfNeeded() || !ConfirmWildPokemonSaved())
            return null;
        int currentRoot = checked((int)(BitConverter.ToUInt32(romData, 0x54B0C) - 0x08000000U));
        int baseRoot = checked((int)(BitConverter.ToUInt32(cfruBaseRom, 0x54B0C) - 0x08000000U));
        var changed = new List<string>();
        var writes = new List<(string Path, string Text)>();
        var binaryWrites = new List<(string Path, byte[] Bytes)>();
        var removals = new List<string>();
        var currentSources = new List<CfruMapSource>();
        var oldItems = new Dictionary<string, CfruItemBall>(StringComparer.Ordinal);
        string manifestPath = Path.Combine(cfruProjectFolder, "maps", "project.json");
        foreach (MapHeader map in mapHeaders.OrderBy(h => h.Bank).ThenBy(h => h.Number))
        {
            CfruMapSource current = CfruMapCodec.Capture(romData, currentRoot, map.Bank, map.Number);
            if (cfruTilesetIds.TryGetValue(current.PrimaryTileset, out uint primaryId)) current.PrimaryTileset = primaryId;
            if (cfruTilesetIds.TryGetValue(current.SecondaryTileset, out uint secondaryId)) current.SecondaryTileset = secondaryId;
            CfruMapSource original = map.Bank < cfruBaseBankCounts.Count && map.Number < cfruBaseBankCounts[map.Bank]
                ? CfruMapCodec.Capture(cfruBaseRom, baseRoot, map.Bank, map.Number) : null;
            string path = CfruMapCodec.SourcePath(cfruProjectFolder, map.Bank, map.Number);
            if (File.Exists(path))
                foreach (CfruItemBall old in CfruMapCodec.Read(path).ItemBalls)
                    oldItems[old.Id] = old;
            CaptureItemBalls(current, original);
            current.Wild = CfruWildCodec.Capture(romData, map.Bank, map.Number, true);
            if (original != null)
                original.Wild = CfruWildCodec.Capture(cfruBaseRom, map.Bank, map.Number, false);
            currentSources.Add(current);
            string currentJson = CfruMapCodec.Serialize(current);
            bool differs = original == null || currentJson != CfruMapCodec.Serialize(original);
            if (!differs)
            {
                if (File.Exists(path)) removals.Add(path);
            }
            else if (!File.Exists(path) || File.ReadAllText(path) != currentJson)
                writes.Add((path, currentJson));
        }
        var tilesets = new Dictionary<(uint Address, bool Secondary), bool>();
        foreach (MapHeader map in mapHeaders)
        {
            MapFooter footer = ReadMapFooter((int)map.FooterAddress);
            if (footer.Tileset1Address != 0) tilesets[(footer.Tileset1Address, false)] = true;
            if (footer.Tileset2Address != 0) tilesets[(footer.Tileset2Address, true)] = true;
        }
        foreach ((uint address, bool secondary) in tilesets.Keys)
        {
            CfruTilesetSource current = CaptureTilesetSource(romData, address, secondary, out byte[] image);
            bool inBase = address < 0x1000000u && (long)address + 24 <= cfruBaseRom.Length &&
                BitConverter.ToUInt32(cfruBaseRom, (int)address + 4) - 0x08000000u < cfruBaseRom.Length;
            CfruTilesetSource original = inBase
                ? CaptureTilesetSource(cfruBaseRom, address, secondary, out _) : null;
            uint identity = cfruTilesetIds.TryGetValue(0x08000000u + address, out uint stable)
                ? stable - 0x08000000u : address;
            current.HeaderAddress = checked((int)identity);
            current.New = !inBase;
            string folder = CfruTilesetCodec.Folder(cfruProjectFolder, (int)identity, secondary);
            string infoPath = Path.Combine(folder, "tileset.json");
            string imagePath = Path.Combine(folder, "tiles.png");
            bool differs = original == null || CfruTilesetCodec.Serialize(current) != CfruTilesetCodec.Serialize(original);
            if (!differs)
            {
                if (File.Exists(infoPath)) removals.Add(infoPath);
                if (File.Exists(imagePath)) removals.Add(imagePath);
            }
            else if (!File.Exists(infoPath) || !File.Exists(imagePath) ||
                File.ReadAllText(infoPath) != CfruTilesetCodec.Serialize(current))
            {
                writes.Add((infoPath, CfruTilesetCodec.Serialize(current)));
                string tempImage = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N") + ".png");
                try
                {
                    CfruTilesetCodec.SaveImage(tempImage, image, current.Palettes, secondary ? 7 : 0, current.TileCount);
                    binaryWrites.Add((imagePath, File.ReadAllBytes(tempImage)));
                }
                finally { if (File.Exists(tempImage)) File.Delete(tempImage); }
            }
        }
        string hiddenItemPath = Path.Combine(cfruProjectFolder, "assembly", "overworld_scripts", "HiddenItem.s");
        string hiddenItemSource = BuildHiddenItemSource(hiddenItemPath, currentSources, oldItems);
        if (hiddenItemSource != null && (!File.Exists(hiddenItemPath) || File.ReadAllText(hiddenItemPath) != hiddenItemSource))
            writes.Add((hiddenItemPath, hiddenItemSource));
        if ((writes.Count > 0 || binaryWrites.Count > 0) && !File.Exists(manifestPath))
            writes.Insert(0, (manifestPath, JsonSerializer.Serialize(new CfruMapManifest
                { BaseBankCounts = cfruBaseBankCounts }) + Environment.NewLine));
        foreach ((string path, string value) in writes)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            string temporary = path + ".tmp";
            File.WriteAllText(temporary, value, new UTF8Encoding(false));
        }
        foreach ((string path, byte[] bytes) in binaryWrites)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllBytes(path + ".tmp", bytes);
        }
        foreach ((string path, _) in writes)
        {
            File.Move(path + ".tmp", path, true);
            changed.Add(Path.GetRelativePath(cfruProjectFolder, path));
        }
        foreach ((string path, _) in binaryWrites)
        {
            File.Move(path + ".tmp", path, true);
            changed.Add(Path.GetRelativePath(cfruProjectFolder, path));
        }
        foreach (string path in removals)
        {
            File.Delete(path);
            changed.Add(Path.GetRelativePath(cfruProjectFolder, path) + " (削除)");
        }
        Dictionary<string, string> mapAssistNow = SnapshotMapAssist();
        foreach (string path in cfruMapAssistHashes.Keys.Union(mapAssistNow.Keys, StringComparer.OrdinalIgnoreCase))
            if (!cfruMapAssistHashes.TryGetValue(path, out string before) ||
                !mapAssistNow.TryGetValue(path, out string after) || before != after)
                changed.Add(Path.GetRelativePath(cfruProjectFolder, path) +
                    (mapAssistNow.ContainsKey(path) ? "" : " (削除)"));
        cfruMapAssistHashes = mapAssistNow;
        return changed;
    }

    private Dictionary<string, string> SnapshotMapAssist()
    {
        string folder = Path.Combine(cfruProjectFolder, "maps", "mapassist");
        if (!Directory.Exists(folder)) return new(StringComparer.OrdinalIgnoreCase);
        return Directory.EnumerateFiles(folder, "*.txt").ToDictionary(p => p,
            p => Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(p))), StringComparer.OrdinalIgnoreCase);
    }

    private CfruTilesetSource CaptureTilesetSource(byte[] data, uint address, bool secondary, out byte[] image)
    {
        byte[] previous = romData;
        try
        {
            romData = data;
            TilesetHeader header = ReadTilesetHeader((int)address);
            MapTilePackage.TilesetPart part = ReadTilesetPart(header, address, secondary, out string error);
            if (part == null) throw new InvalidDataException(error ?? "タイルセットを読み取れません。");
            image = part.TileImage;
            byte[] rawHeader = new byte[24];
            Buffer.BlockCopy(data, (int)address, rawHeader, 0, 24);
            Array.Clear(rawHeader, 4, 12);
            Array.Clear(rawHeader, 20, 4);
            return new CfruTilesetSource
            {
                HeaderAddress = (int)address,
                Secondary = secondary,
                Header = rawHeader,
                TileCount = part.TileCount,
                BlockCount = part.BlockCount,
                BlockBytes = MapEditor.BLOCK_DATA_SIZE,
                BehaviorBytes = GameProfile.Current.BehaviorBytes,
                Compressed = part.Compressed,
                ImageSha256 = CfruTilesetCodec.Sha256(image),
                Palettes = part.Palettes,
                Blocks = part.Blocks,
                Behaviors = part.Behaviors
            };
        }
        finally { romData = previous; }
    }

    private List<int> ReadBankCountsFromLoadedMaps()
    {
        var counts = new List<int>();
        foreach (IGrouping<int, MapHeader> bank in mapHeaders.GroupBy(h => h.Bank).OrderBy(g => g.Key))
        {
            if (bank.Key != counts.Count)
                throw new InvalidDataException("ベース ROM のマップバンク番号が連続していません。");
            counts.Add(bank.Max(h => h.Number) + 1);
        }
        return counts;
    }

    private void CaptureItemBalls(CfruMapSource current, CfruMapSource original)
    {
        for (int index = 0; index < current.Persons.Length / 24; index++)
        {
            int at = index * 24;
            uint script = BitConverter.ToUInt32(current.Persons, at + 16);
            int item = script >= 0x08000000u ? ItemBallItemAt(script - 0x08000000u) : -1;
            if (item < 0) continue;
            if (cfruManagedItems.TryGetValue(script, out CfruItemBall managed))
            {
                current.ItemBalls.Add(new CfruItemBall
                    { Id = managed.Id, Label = managed.Label, ItemId = item, PersonIndex = index });
                Array.Clear(current.Persons, at + 16, 4);
                continue;
            }
            bool inBase = false;
            if (original != null)
            {
                ushort flag = BitConverter.ToUInt16(current.Persons, at + 20);
                for (int old = 0; old < original.Persons.Length / 24; old++)
                {
                    int oldAt = old * 24;
                    if (BitConverter.ToUInt16(original.Persons, oldAt + 20) == flag &&
                        BitConverter.ToUInt32(original.Persons, oldAt + 16) == script)
                    { inBase = true; break; }
                }
            }
            if (inBase) continue;
            string id = Guid.NewGuid().ToString("N");
            var created = new CfruItemBall
            {
                Id = id,
                Label = $"HiddenItem_{current.Bank:D2}_{current.Number:D2}_{id}",
                ItemId = item,
                PersonIndex = index
            };
            current.ItemBalls.Add(created);
            cfruManagedItems[script] = created;
            Array.Clear(current.Persons, at + 16, 4);
        }
    }

    private static string BuildHiddenItemSource(string path, IEnumerable<CfruMapSource> maps,
        IDictionary<string, CfruItemBall> oldItems)
    {
        string existing = File.Exists(path) ? File.ReadAllText(path) : string.Empty;
        var pattern = new Regex(@"(?ms)^@ BBE-BEGIN (?<id>[0-9a-fA-F]{32})\r?\n.*?^@ BBE-END \k<id>\r?\n?");
        var oldBlocks = pattern.Matches(existing).Cast<Match>()
            .ToDictionary(m => m.Groups["id"].Value, m => m.Value, StringComparer.Ordinal);
        string manual = pattern.Replace(existing, string.Empty);
        if (Regex.Matches(existing, @"(?m)^@ BBE-BEGIN ").Count != oldBlocks.Count)
            throw new InvalidDataException("HiddenItem.s の生成範囲の終端が見つかりません。");
        var items = maps.SelectMany(m => m.ItemBalls).OrderBy(i => i.Label, StringComparer.Ordinal).ToList();
        if (items.Count == 0 && existing.Length == 0) return null;
        if (items.Count == oldBlocks.Count && items.All(i => oldBlocks.ContainsKey(i.Id) &&
            oldItems.TryGetValue(i.Id, out CfruItemBall old) && old.ItemId == i.ItemId && old.Label == i.Label))
            return existing;
        if (manual.Length == 0) manual = ".text\n.thumb\n.align 2\n";
        var text = new StringBuilder(manual.TrimEnd('\r', '\n'));
        foreach (CfruItemBall item in items)
        {
            text.Append("\n\n");
            if (oldBlocks.TryGetValue(item.Id, out string block) &&
                oldItems.TryGetValue(item.Id, out CfruItemBall old) && old.ItemId == item.ItemId)
            {
                text.Append(block.TrimEnd('\r', '\n'));
                continue;
            }
            text.Append("@ BBE-BEGIN ").Append(item.Id).Append('\n');
            text.Append(".global ").Append(item.Label).Append('\n');
            text.Append(item.Label).Append(":\n");
            text.Append("    .byte 0x1A, 0x00, 0x80, ")
                .AppendFormat("0x{0:X2}, 0x{1:X2}\n", item.ItemId & 0xFF, item.ItemId >> 8);
            text.Append("    .byte 0x1A, 0x01, 0x80, 0x01, 0x00, 0x09, 0x01, 0x02\n");
            text.Append("@ BBE-END ").Append(item.Id);
        }
        return text.Append('\n').ToString();
    }
}
