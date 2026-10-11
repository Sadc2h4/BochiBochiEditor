using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace BochiBochiEditor;

// A map source contains ROM structures, rather than a patch at fixed ROM offsets.
// Pointer fields between the structures are rebuilt when the source is loaded.
internal sealed class CfruMapSource
{
    public int Format { get; set; } = 1;
    public int Bank { get; set; }
    public int Number { get; set; }
    public byte[] Header { get; set; } = Array.Empty<byte>();
    public byte[] Footer { get; set; } = Array.Empty<byte>();
    public byte[] Layout { get; set; } = Array.Empty<byte>();
    public byte[] Border { get; set; } = Array.Empty<byte>();
    public uint PrimaryTileset { get; set; }
    public uint SecondaryTileset { get; set; }
    public byte[] MapName { get; set; } = Array.Empty<byte>();
    public byte[] EventHeader { get; set; } = Array.Empty<byte>();
    public byte[] Persons { get; set; } = Array.Empty<byte>();
    public byte[] Warps { get; set; } = Array.Empty<byte>();
    public byte[] Traps { get; set; } = Array.Empty<byte>();
    public byte[] Signs { get; set; } = Array.Empty<byte>();
    public byte[] ConnectionsHeader { get; set; } = Array.Empty<byte>();
    public byte[] Connections { get; set; } = Array.Empty<byte>();
    public byte[] MapScripts { get; set; } = Array.Empty<byte>();
    public List<CfruMapScriptList> MapScriptLists { get; set; } = new();
    public List<CfruItemBall> ItemBalls { get; set; } = new();
    public List<CfruWildTime> Wild { get; set; } = new();
}

internal sealed class CfruWildTime
{
    public int Time { get; set; }
    public List<CfruWildArea> Areas { get; set; } = new();
}

internal sealed class CfruWildArea
{
    public byte Rate { get; set; }
    public byte[] Slots { get; set; } = Array.Empty<byte>();
}

internal sealed class CfruItemBall
{
    public string Id { get; set; } = "";
    public int PersonIndex { get; set; }
    public int ItemId { get; set; }
    public string Label { get; set; } = "";
}

internal sealed class CfruMapScriptList
{
    public int TableOffset { get; set; }
    public byte[] Data { get; set; } = Array.Empty<byte>();
}

internal sealed class CfruMapManifest
{
    public int Format { get; set; } = 1;
    public List<int> BaseBankCounts { get; set; } = new();
}

internal static class CfruMapCodec
{
    private const uint RomBase = 0x08000000;
    private const int MaxMapCells = 255 * 255;
    private const int MaxEvents = 255;
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };

    public static string SourcePath(string folder, int bank, int number) =>
        Path.Combine(folder, "maps", bank.ToString("D2"), number.ToString("D2") + ".json");

    public static string Serialize(CfruMapSource source) => JsonSerializer.Serialize(source, JsonOptions) + Environment.NewLine;

    public static CfruMapSource Read(string path)
    {
        CfruMapSource source = JsonSerializer.Deserialize<CfruMapSource>(File.ReadAllText(path))
            ?? throw new InvalidDataException("マップソースが空です: " + path);
        if (source.Format != 1 || source.Bank is < 0 or > 255 || source.Number is < 0 or > 254 ||
            source.Header?.Length != 28 || source.Footer?.Length != 28)
            throw new InvalidDataException("マップソースの形式が不正です: " + path);
        return source;
    }

    public static CfruMapSource Capture(byte[] rom, int bankTable, int bank, int number)
    {
        int bankList = PointerAt(rom, bankTable + bank * 4);
        int headerAt = PointerAt(rom, bankList + number * 4);
        var source = new CfruMapSource { Bank = bank, Number = number, Header = Copy(rom, headerAt, 28) };
        int footerAt = PointerAt(rom, headerAt);
        source.Footer = Copy(rom, footerAt, 28);
        int width = source.Footer[0];
        int height = source.Footer[4];
        int borderWidth = source.Footer[24];
        int borderHeight = source.Footer[25];
        if (width == 0 || height == 0 || width * height > MaxMapCells || borderWidth == 0 || borderHeight == 0)
            throw new InvalidDataException($"マップ ({bank}, {number}) のサイズが不正です。");
        source.Layout = Copy(rom, PointerAt(rom, footerAt + 12), width * height * 2);
        source.Border = Copy(rom, PointerAt(rom, footerAt + 8), borderWidth * borderHeight * 2);
        source.PrimaryTileset = BitConverter.ToUInt32(source.Footer, 16);
        source.SecondaryTileset = BitConverter.ToUInt32(source.Footer, 20);
        int nameId = source.Header[20] - 0x58;
        if (nameId >= 0 && nameId < 109)
        {
            int nameAt = PointerAt(rom, PointerAt(rom, 0xC1E78) + nameId * 4);
            int length = 0;
            while (length < 64 && nameAt + length < rom.Length && rom[nameAt + length] != 0xFF) length++;
            if (length == 64 || nameAt + length >= rom.Length)
                throw new InvalidDataException("Map name is not terminated.");
            source.MapName = Copy(rom, nameAt, length + 1);
        }
        Array.Clear(source.Footer, 8, 16);

        int eventsAt = OptionalPointerAt(rom, headerAt + 4);
        if (eventsAt != 0)
        {
            source.EventHeader = Copy(rom, eventsAt, 20);
            byte[] counts = source.EventHeader;
            source.Persons = ReadEventList(rom, eventsAt + 4, counts[0], 24);
            source.Warps = ReadEventList(rom, eventsAt + 8, counts[1], 8);
            source.Traps = ReadEventList(rom, eventsAt + 12, counts[2], 16);
            source.Signs = ReadEventList(rom, eventsAt + 16, counts[3], 12);
            Array.Clear(source.EventHeader, 4, 16);
        }

        int connectionsAt = OptionalPointerAt(rom, headerAt + 12);
        if (connectionsAt != 0)
        {
            source.ConnectionsHeader = Copy(rom, connectionsAt, 8);
            source.Connections = ReadEventList(rom, connectionsAt + 4, source.ConnectionsHeader[0], 12);
            Array.Clear(source.ConnectionsHeader, 4, 4);
        }

        int scriptsAt = OptionalPointerAt(rom, headerAt + 8);
        if (scriptsAt != 0)
        {
            var table = new List<byte>();
            for (int n = 0; n < MaxEvents; n++)
            {
                byte type = Copy(rom, scriptsAt + table.Count, 1)[0];
                if (type == 0) { table.Add(0); break; }
                int entryOffset = table.Count;
                table.AddRange(Copy(rom, scriptsAt + entryOffset, 5));
                if (type is 2 or 4)
                {
                    int listAt = OptionalPointerAt(rom, scriptsAt + entryOffset + 1);
                    if (listAt != 0)
                    {
                        var entries = new List<byte>();
                        for (int k = 0; k < MaxEvents; k++)
                        {
                            byte[] entry = Copy(rom, listAt + k * 8, 8);
                            if (BitConverter.ToUInt16(entry, 0) == 0) { entries.AddRange(entry); break; }
                            entries.AddRange(entry);
                        }
                        source.MapScriptLists.Add(new CfruMapScriptList { TableOffset = entryOffset + 1, Data = entries.ToArray() });
                        table[entryOffset + 1] = table[entryOffset + 2] = table[entryOffset + 3] = table[entryOffset + 4] = 0;
                    }
                }
            }
            if (table.Count == 0 || table[^1] != 0)
                throw new InvalidDataException($"マップ ({bank}, {number}) のスクリプト表が終端しません。");
            source.MapScripts = table.ToArray();
        }
        Array.Clear(source.Header, 0, 16);
        return source;
    }

    public static int Apply(byte[] rom, int bankTable, IList<int> baseBankCounts, IEnumerable<CfruMapSource> maps,
        int allocationStart, IDictionary<uint, CfruItemBall> managedItems = null,
        IDictionary<uint, uint> tilesetPointers = null)
    {
        var allocator = new Allocator(rom, allocationStart);
        var bankCounts = baseBankCounts.ToList();
        var names = new Dictionary<int, byte[]>();
        foreach (CfruMapSource source in maps.OrderBy(m => m.Bank).ThenBy(m => m.Number))
        {
            if (source.Format != 1 || source.Header.Length != 28 || source.Footer.Length != 28)
                throw new InvalidDataException($"マップ ({source.Bank}, {source.Number}) の形式が不正です。");
            if (source.Layout.Length != source.Footer[0] * source.Footer[4] * 2 ||
                source.Border.Length != source.Footer[24] * source.Footer[25] * 2)
                throw new InvalidDataException($"Map layout size is invalid at ({source.Bank}, {source.Number}).");
            byte[] footer = (byte[])source.Footer.Clone();
            WritePointer(footer, 8, allocator.Put(source.Border));
            WritePointer(footer, 12, allocator.Put(source.Layout));
            WriteRawPointer(footer, 16, tilesetPointers != null && tilesetPointers.TryGetValue(source.PrimaryTileset, out uint first) ? first : source.PrimaryTileset);
            WriteRawPointer(footer, 20, tilesetPointers != null && tilesetPointers.TryGetValue(source.SecondaryTileset, out uint second) ? second : source.SecondaryTileset);
            int footerAt = allocator.Put(footer);

            int eventAt = 0;
            if (source.EventHeader.Length != 0)
            {
                byte[] events = (byte[])source.EventHeader.Clone();
                if (events.Length != 20 || source.Persons.Length != events[0] * 24 ||
                    source.Warps.Length != events[1] * 8 || source.Traps.Length != events[2] * 16 ||
                    source.Signs.Length != events[3] * 12)
                    throw new InvalidDataException("イベント表の件数とデータ長が一致しません。");
                byte[] persons = (byte[])source.Persons.Clone();
                foreach (CfruItemBall item in source.ItemBalls)
                {
                    if (item.PersonIndex < 0 || item.PersonIndex >= events[0] || item.ItemId is < 1 or > 65535)
                        throw new InvalidDataException("落とし物の参照先が不正です。");
                    int scriptAt = allocator.Put(BuildItemBallScript(item.ItemId));
                    WritePointer(persons, item.PersonIndex * 24 + 16, scriptAt);
                    managedItems?.Add(0x08000000u + (uint)scriptAt, item);
                }
                WritePointer(events, 4, allocator.Put(persons));
                WritePointer(events, 8, allocator.Put(source.Warps));
                WritePointer(events, 12, allocator.Put(source.Traps));
                WritePointer(events, 16, allocator.Put(source.Signs));
                eventAt = allocator.Put(events);
            }

            int connectionAt = 0;
            if (source.ConnectionsHeader.Length != 0)
            {
                byte[] connections = (byte[])source.ConnectionsHeader.Clone();
                if (connections.Length != 8 || source.Connections.Length != connections[0] * 12)
                    throw new InvalidDataException("接続表の件数とデータ長が一致しません。");
                WritePointer(connections, 4, allocator.Put(source.Connections));
                connectionAt = allocator.Put(connections);
            }

            int scriptsAt = 0;
            if (source.MapScripts.Length != 0)
            {
                byte[] scripts = (byte[])source.MapScripts.Clone();
                foreach (CfruMapScriptList list in source.MapScriptLists)
                {
                    if (list.TableOffset < 1 || list.TableOffset + 4 > scripts.Length)
                        throw new InvalidDataException("マップスクリプト一覧の参照位置が不正です。");
                    WritePointer(scripts, list.TableOffset, allocator.Put(list.Data));
                }
                scriptsAt = allocator.Put(scripts);
            }

            byte[] header = (byte[])source.Header.Clone();
            WritePointer(header, 0, footerAt);
            WritePointer(header, 4, eventAt);
            WritePointer(header, 8, scriptsAt);
            WritePointer(header, 12, connectionAt);
            int headerAt = allocator.Put(header);
            bankTable = InstallHeader(rom, allocator, bankTable, bankCounts, source.Bank, source.Number, headerAt);
            int nameId = header[20] - 0x58;
            if (nameId >= 0 && nameId < 109 && source.MapName?.Length > 0)
            {
                if (source.MapName[^1] != 0xFF || source.MapName.Length > 64)
                    throw new InvalidDataException("Invalid map name bytes.");
                if (names.TryGetValue(nameId, out byte[] oldName) && !oldName.SequenceEqual(source.MapName))
                    throw new InvalidDataException($"Conflicting map name ID {nameId + 0x58:X2}.");
                names[nameId] = source.MapName;
                int nameTable = PointerAt(rom, 0xC1E78);
                WritePointer(rom, nameTable + nameId * 4, allocator.Put(source.MapName));
            }
        }
        return allocator.Next;
    }

    private static int InstallHeader(byte[] rom, Allocator allocator, int bankTable, IList<int> bankCounts, int bank, int number, int header)
    {
        if (bank is < 0 or > 255 || number is < 0 or > 254)
            throw new InvalidDataException("マップ番号が範囲外です。");
        int bankCount = bankCounts.Count;
        if (bank > bankCount) throw new InvalidDataException("バンク番号を飛ばして追加できません。");
        if (bank == bankCount)
        {
            if (number != 0) throw new InvalidDataException("新しいバンクはマップ 0 から始めてください。");
            byte[] expanded = Copy(rom, bankTable, bankCount * 4);
            Array.Resize(ref expanded, (bankCount + 2) * 4);
            int newTable = allocator.Put(expanded);
            int newList = allocator.Put(new byte[8]);
            WritePointer(rom, newList, header);
            WritePointer(rom, newTable + bank * 4, newList);
            ReplaceAlignedReferences(rom, bankTable, newTable);
            WritePointer(rom, 0x54B0C, newTable);
            bankCounts.Add(1);
            return newTable;
        }
        int listAt = PointerAt(rom, bankTable + bank * 4);
        int count = bankCounts[bank];
        if (number > count) throw new InvalidDataException("マップ番号を飛ばして追加できません。");
        if (number == count)
        {
            byte[] expanded = Copy(rom, listAt, count * 4);
            Array.Resize(ref expanded, (count + 2) * 4);
            int newList = allocator.Put(expanded);
            WritePointer(rom, newList + number * 4, header);
            ReplaceAlignedReferences(rom, listAt, newList);
            WritePointer(rom, bankTable + bank * 4, newList);
            bankCounts[bank]++;
        }
        else WritePointer(rom, listAt + number * 4, header);
        return bankTable;
    }

    private static byte[] ReadEventList(byte[] rom, int pointerAt, int count, int entrySize) =>
        count == 0 ? Array.Empty<byte>() : Copy(rom, PointerAt(rom, pointerAt), checked(count * entrySize));

    private static byte[] BuildItemBallScript(int item) => new byte[]
    {
        0x1A, 0x00, 0x80, (byte)item, (byte)(item >> 8),
        0x1A, 0x01, 0x80, 0x01, 0x00, 0x09, 0x01, 0x02
    };

    private static void ReplaceAlignedReferences(byte[] rom, int oldTarget, int newTarget)
    {
        uint oldPointer = RomBase + (uint)oldTarget;
        uint newPointer = RomBase + (uint)newTarget;
        int limit = Math.Min(rom.Length, 0x1000000);
        for (int at = 0; at + 4 <= limit; at += 4)
            if (BitConverter.ToUInt32(rom, at) == oldPointer)
                WriteRawPointer(rom, at, newPointer);
    }

    private static byte[] Copy(byte[] rom, int offset, int length)
    {
        if (offset < 0 || length < 0 || (long)offset + length > rom.Length)
            throw new InvalidDataException("マップデータが ROM の範囲外です。");
        byte[] result = new byte[length];
        Buffer.BlockCopy(rom, offset, result, 0, length);
        return result;
    }

    private static int OptionalPointerAt(byte[] rom, int offset)
    {
        if (offset < 0 || offset + 4 > rom.Length) return 0;
        uint pointer = BitConverter.ToUInt32(rom, offset);
        uint target = pointer - RomBase;
        return pointer >= RomBase && target < rom.Length ? (int)target : 0;
    }

    private static int PointerAt(byte[] rom, int offset) => OptionalPointerAt(rom, offset) is int at and > 0
        ? at : throw new InvalidDataException("マップの参照先が ROM の範囲外です。");

    private static void WritePointer(byte[] bytes, int offset, int target) =>
        WriteRawPointer(bytes, offset, target == 0 ? 0u : RomBase + (uint)target);

    private static void WriteRawPointer(byte[] bytes, int offset, uint value) =>
        Buffer.BlockCopy(BitConverter.GetBytes(value), 0, bytes, offset, 4);

    private sealed class Allocator
    {
        private readonly byte[] rom;
        private int next;
        public Allocator(byte[] rom, int start) { this.rom = rom; next = start; }
        public int Next => next;
        public int Put(byte[] data)
        {
            if (data == null || data.Length == 0) return 0;
            next = (next + 3) & ~3;
            if ((long)next + data.Length > rom.Length)
                throw new InvalidDataException("マップデータの配置先が ROM の範囲外です。");
            for (int i = 0; i < data.Length; i++)
                if (rom[next + i] != 0xFF)
                    throw new InvalidDataException("マップデータの配置先に既存データがあります。");
            int offset = next;
            Buffer.BlockCopy(data, 0, rom, offset, data.Length);
            next += data.Length;
            return offset;
        }
    }
}
