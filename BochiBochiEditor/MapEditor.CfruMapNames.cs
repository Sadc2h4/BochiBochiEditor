using System;
using System.IO;
using System.Linq;

namespace BochiBochiEditor;

public sealed record CfruMapNameInfo(byte NameId, string Text, int SharedMapCount);

public partial class MapEditor
{
    // Names belong to a name ID, which can be shared by several maps.
    public CfruMapNameInfo GetCfruMapName(int bank, int number)
    {
        if (!IsCfruProject || romData == null)
            throw new InvalidOperationException("CFRU プロジェクトを開いてください。");
        MapHeader map = mapHeaders.FirstOrDefault(h => h.Bank == bank && h.Number == number)
            ?? throw new ArgumentOutOfRangeException(nameof(number), "マップが見つかりません。");
        int index = map.MapNameId - MAP_NAME_FIRST_INDEX;
        long pointerOffset = GetMapNamePointerOffset(index);
        if (pointerOffset < 0)
            throw new InvalidDataException("このマップには編集できるマップ名 ID がありません。");
        uint pointer = BitConverter.ToUInt32(romData, (int)pointerOffset);
        long address = pointer - 0x08000000L;
        if (pointer < 0x08000000u || address < 0 || address >= romData.Length)
            throw new InvalidDataException("マップ名の参照先が不正です。");
        int length = 0;
        while (length < 64 && address + length < romData.Length && romData[address + length] != 0xFF)
            length++;
        if (length == 64 || address + length >= romData.Length)
            throw new InvalidDataException("マップ名の終端が見つかりません。");
        string name = TextConverter.BytesToPokemonString(romData, (int)address, length + 1);
        return new CfruMapNameInfo(map.MapNameId, name,
            mapHeaders.Count(h => h.MapNameId == map.MapNameId));
    }

    // The preview ROM is changed in memory. SaveCfruProject writes the map JSON.
    public CfruMapNameInfo RenameCfruMapName(int bank, int number, string newName)
    {
        CfruMapNameInfo current = GetCfruMapName(bank, number);
        if (hasUnsavedChanges)
            throw new InvalidOperationException("編集中のマップを確定してからマップ名を変更してください。");
        if (string.IsNullOrWhiteSpace(newName) || newName != newName.Trim() ||
            newName.IndexOfAny(new[] { '\r', '\n' }) >= 0)
            throw new ArgumentException("マップ名を1行で入力してください。", nameof(newName));

        byte[] encoded = EncodeNewMapName(newName);
        if (encoded.Length > GameProfile.Current.MapNameMaxLength + 1 ||
            TextConverter.BytesToPokemonString(encoded, 0, encoded.Length) != newName)
            throw new ArgumentException("マップ名が長すぎるか、使えない文字が含まれています。", nameof(newName));
        if (current.Text == newName)
            return current;

        int index = current.NameId - MAP_NAME_FIRST_INDEX;
        int address = FindCfruNameSpace(encoded.Length);
        string error = WriteNewMapName(index, encoded, (uint)address);
        if (error != null)
            throw new InvalidDataException(error);

        string label = $"[{current.NameId:X2}]{newName}";
        cmbMapNameId.Items[index] = label;
        cmbNewMapName.Items[index] = newName;
        ApplyMapSearchFilter();
        if (tempHeader != null && tempHeader.MapNameId == current.NameId)
            lblCurrentMap.Text = string.Format(Localizer.T("現在マップ : ({0}, {1}) {2}"),
                tempHeader.Bank, tempHeader.Number, tempHeader.GetMapName(this));
        return GetCfruMapName(bank, number);
    }

    private int FindCfruNameSpace(int length)
    {
        // Only the in-memory preview uses this area. The build chooses its own address.
        for (int at = 0x1F00000; (long)at + length <= romData.Length; at += 4)
        {
            bool free = true;
            for (int i = 0; i < length; i++)
                if (romData[at + i] != 0xFF) { free = false; break; }
            if (free) return at;
        }
        throw new InvalidDataException("マップ名の作業領域に空きがありません。");
    }
}
