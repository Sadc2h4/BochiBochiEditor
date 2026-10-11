using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BochiBochiEditor;

// The editor's existing wild encounter form edits ROM-shaped tables in memory.
// These addresses are preview storage only; map JSON contains the actual source.
internal static class CfruWildCodec
{
    private const uint RomBase = 0x08000000;
    private static readonly int[] TableOffsets = { 0x1C00000, 0x1C30000, 0x1C10000, 0x1C20000 };
    private static readonly int[] SlotCounts = { 12, 5, 5, 10 };
    internal static bool EditorMode { get; set; }
    internal static int TableOffset(int time) => TableOffsets[time];

    internal static List<CfruWildTime> Capture(byte[] rom, int bank, int number, bool preview)
    {
        var result = new List<CfruWildTime>();
        for (int time = 0; time < 4; time++)
        {
            if (!preview && time != 1) continue;
            int start = preview ? TableOffsets[time] : Pointer(rom, 0x8257C);
            for (int at = start, n = 0; n < 512; at += 20, n++)
            {
                if (at < 0 || at + 20 > rom.Length) throw new InvalidDataException("Wild encounter table is out of range.");
                if (rom[at] == 0xFF && rom[at + 1] == 0xFF) break;
                if (rom[at] != bank || rom[at + 1] != number) continue;
                var entry = new CfruWildTime { Time = time };
                for (int area = 0; area < 4; area++)
                {
                    int info = Pointer(rom, at + 4 + area * 4, true);
                    if (info == 0) { entry.Areas.Add(new CfruWildArea()); continue; }
                    int slots = Pointer(rom, info + 4);
                    var bytes = new byte[SlotCounts[area] * 4];
                    Buffer.BlockCopy(rom, slots, bytes, 0, bytes.Length);
                    entry.Areas.Add(new CfruWildArea { Rate = rom[info], Slots = bytes });
                }
                result.Add(entry);
                break;
            }
        }
        return result;
    }

    internal static void ApplyPreview(byte[] rom, IEnumerable<CfruMapSource> sources)
    {
        var tables = new List<(int Bank, int Number, byte[] Raw, CfruWildTime Source)>[4];
        for (int time = 0; time < 4; time++) tables[time] = new();
        int day = Pointer(rom, 0x8257C);
        for (int at = day, n = 0; n < 512; at += 20, n++)
        {
            if (at + 20 > rom.Length) throw new InvalidDataException("Base wild encounter table is out of range.");
            if (rom[at] == 0xFF && rom[at + 1] == 0xFF) break;
            var raw = new byte[20];
            Buffer.BlockCopy(rom, at, raw, 0, 20);
            tables[1].Add((raw[0], raw[1], raw, null));
        }
        foreach (CfruMapSource map in sources)
        {
            foreach (var table in tables) table.RemoveAll(e => e.Bank == map.Bank && e.Number == map.Number);
            foreach (CfruWildTime wild in map.Wild ?? new())
            {
                if (wild.Time is < 0 or > 3 || wild.Areas?.Count != 4)
                    throw new InvalidDataException($"Invalid wild encounter data for ({map.Bank}, {map.Number}).");
                tables[wild.Time].Add((map.Bank, map.Number, null, wild));
            }
        }
        int next = 0x1C40000;
        for (int time = 0; time < 4; time++)
        {
            int start = TableOffsets[time];
            if (tables[time].Count >= 512) throw new InvalidDataException("Too many wild encounter maps.");
            for (int index = 0; index < tables[time].Count; index++)
            {
                var entry = tables[time][index];
                int at = start + index * 20;
                if (entry.Raw != null) { Buffer.BlockCopy(entry.Raw, 0, rom, at, 20); continue; }
                Array.Clear(rom, at, 20);
                rom[at] = checked((byte)entry.Bank);
                rom[at + 1] = checked((byte)entry.Number);
                for (int area = 0; area < 4; area++)
                {
                    CfruWildArea info = entry.Source.Areas[area];
                    if (info.Slots == null || info.Slots.Length == 0) continue;
                    if (info.Slots.Length != SlotCounts[area] * 4)
                        throw new InvalidDataException("Invalid wild encounter slot count.");
                    next = (next + 3) & ~3;
                    int infoAt = next;
                    int slotsAt = infoAt + 8;
                    next = slotsAt + info.Slots.Length;
                    if (next > 0x1D00000) throw new InvalidDataException("Wild encounter preview is full.");
                    rom[infoAt] = info.Rate;
                    WritePointer(rom, infoAt + 4, slotsAt);
                    Buffer.BlockCopy(info.Slots, 0, rom, slotsAt, info.Slots.Length);
                    WritePointer(rom, at + 4 + area * 4, infoAt);
                }
            }
            int end = start + tables[time].Count * 20;
            rom[end] = rom[end + 1] = 0xFF;
        }
        WritePointer(rom, 0x8257C, TableOffsets[1]);
    }

    private static int Pointer(byte[] rom, int at, bool optional = false)
    {
        uint value = BitConverter.ToUInt32(rom, at);
        if (optional && value == 0) return 0;
        long target = value - (long)RomBase;
        if (target < 0 || target >= rom.Length) throw new InvalidDataException($"Invalid wild encounter pointer {value:X8}.");
        return (int)target;
    }

    private static void WritePointer(byte[] rom, int at, int target) =>
        Buffer.BlockCopy(BitConverter.GetBytes(RomBase + (uint)target), 0, rom, at, 4);
}
