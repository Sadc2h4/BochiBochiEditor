# CFRU モードの UI 接続

このブランチは保存・読み込み処理だけを含む。画面側は `MapEditor` の公開メソッドを呼ぶ。

## 開く・保存する

1. ベース ROM（BPRJ、16 または 32 MiB）を選び、次に NeoDark-CFRU フォルダを選ぶ。
2. `OpenCfruProject(baseRomPath, cfruFolder)` を呼ぶ。`IsCfruProject` が `true` なら CFRU モード。
3. 既存の保存ボタンでは、CFRU モードなら `SaveCfruProject()` を呼ぶ。戻り値は CFRU フォルダからの相対パス一覧。`null` は保存確認のキャンセル、空一覧は変更なし。パスをコピーできるテキスト欄に表示する。

CFRU モードでは従来の `.gba` 保存処理を呼ばない。従来の ROM 編集モードは既存の開く・保存する処理を使う。

## マップ名を変える

選択中のマップを「確定」してから、次を呼ぶ。

```csharp
var before = GetCfruMapName(bank, number);
var after = RenameCfruMapName(bank, number, newName);
// その後、通常の SaveCfruProject() で JSON を出力する。
```

`CfruMapNameInfo` の `Text` が表示名、`NameId` がゲーム内の名前 ID、`SharedMapCount` が同じ ID を使うマップ数。名前 ID は共有されるため、変更すると同じ ID のマップ名も変わる。`SharedMapCount` が 2 以上なら UI でその旨を示す。

`RenameCfruMapName` は作業用 ROM のメモリとマップ一覧を更新する。アドレス入力は不要。空欄、未対応文字、長すぎる名前、未確定のマップ編集は例外になるので、UI でメッセージを表示する。ベース ROM のファイルは変更しない。
