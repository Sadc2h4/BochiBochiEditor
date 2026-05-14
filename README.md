# BochiBochiEditor CLI棚卸

## 現在の機能棚卸

▼マップ編集エディタに進む/戻るを追加，ミニコンソール追加
<img width="2210" height="1348" alt="image" src="https://github.com/user-attachments/assets/a4c8bb8e-f540-44cb-8b36-41063733ad8e" />

▼スクリプトのポインタアドレス取得したい
<img width="1162" height="629" alt="image" src="https://github.com/user-attachments/assets/14e6eb4b-8863-4f72-b472-36853a8182d4" />


### 実装済みエディター
- ポケモン
- TM/HM・教え技
- タマゴ技
- 図鑑順
- 生息地
- 図鑑リスト
- アイテム
- アイテム使用表示位置
- トレーナー画像/肩書き
- トレーナーデータ
- NPCポケモン交換
- メール内容
- マップ
- 歩行グラフィック
- 野生ポケモン
- 空き領域検索

### ボタンはあるが未接続
- タイルアニメ＆ドア
- タウンマップ

## CLI対応方針

### 今回対応した内容
- `features`
  現在の機能棚卸とCLI対応状況の出力
- `rom-info`
  ROMタイトル、コード、サイズ、SHA-256の出力
- `find-free-space`
  空き領域検索
- `decode-text`
  指定オフセットの文字列デコード
- `export-pokemon-names`
  ポケモン名一覧の書き出し
- `export-item-names`
  アイテム名一覧の書き出し
- `export-move-names`
  技名一覧の書き出し
- `export-trainer-class-names`
  トレーナー肩書き一覧の書き出し
- `item-info`
  個別アイテム情報の取得
- `pokemon-stats`
  種族値・努力値・タイプ・特性・持ち物・タマゴ関連・捕獲率などの読取
- `export-pokemon-stats-csv`
  種族値関連の一括編集用CSVを書き出し
- `import-pokemon-stats-csv`
  `export-pokemon-stats-csv` で出した列構成のCSVを一括反映
- `update-pokemon-stats`
  `pokemon-stats` 対象の基本更新
  基礎値6種、努力値6種、タイプ、特性、持ち物、性別値、タマゴ歩数、タマゴグループ、捕獲率、なつき度、成長率、逃走率、体色、図鑑向きに対応
- `trainer-info`
  トレーナーデータ本体と手持ち一覧の読取
- `update-trainer`
  トレーナー本体の基本更新
  肩書ID、BGM、スプライトID、名前、所持アイテム4枠、ダブルフラグ、AI、未知値、手持ち1枠のポケモン/レベル/IV/未知値/アイテム/技更新に対応
  `--data-type` と `--pokemon-count` による手持ち構造変更、`--start` / `--pokemon-data-address` による再配置にも対応
- `import-images`
  画像導入CLI
  `pokemon-sprite` は 256x64 の4分割画像から正面/背面/通常パレット/色違いパレットを一括導入
  `item-image` は 24x24 の画像とパレットを一括導入
  `pokemon-icon` は 32x64 の手持ちアイコン画像を導入し、必要に応じてパレットIDも更新
  `trainer-image` は 64x64 のトレーナー画像とパレットを一括導入
  `--vanilla` 指定時は `PokedexOrder` を使って全国図鑑順IDから内部IDへ変換
  `--neworder` 指定時はエディタ上の並びをそのまま使用
- `export-images`
  ROM内画像の一括エクスポート
  `pokemon-sprite` は import 可能な 256x64 形式で出力
  `item-image` / `pokemon-icon` / `trainer-image` もPNGで出力
- `export-image-sheet`
  画像一覧シート出力
  1行16マスで `pokemon-sprite` / `pokemon-icon` / `item-image` / `trainer-image` をPNG化
  `pokemon-sprite` は `front-normal` / `front-shiny` / `back-normal` / `back-shiny`
  `pokemon-icon` は `frame1` / `frame2` / `full` を選択可能

### 次段で分離対象にする内容
- ポケモン詳細編集
- TM/HM・教え技編集
- タマゴ技編集
- 図鑑順/図鑑リスト編集
- トレーナーデータ再配置
- ポケモン詳細の名称/図鑑/進化/技範囲
- NPCポケモン交換編集
- メール内容編集
- 野生ポケモン編集
- マップ編集
- 歩行グラフィック編集

## CLI利用例

```powershell
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- features --format text
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- rom-info --rom .\test700000.gba
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- export-pokemon-names --rom .\test700000.gba --format text --out .\out\pokemon_names.txt
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- find-free-space --rom .\test700000.gba --start 0x700000 --length 2048
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- pokemon-stats --rom .\test700000.gba --pokemon 25 --out .\out\pokemon_25.json
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- export-pokemon-stats-csv --rom .\test700000.gba --csv-out .\out\pokemon_stats.csv
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- import-pokemon-stats-csv --rom .\test700000.gba --csv .\out\pokemon_stats.csv --out-rom .\out\test_pokemon_csv_edit.gba --out .\out\pokemon_stats_csv_import.json
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- update-pokemon-stats --rom .\test700000.gba --pokemon 25 --hp 36 --speed 95 --ability1 9 --out-rom .\out\test_pokemon_edit.gba --out .\out\pokemon_25_update.json
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- trainer-info --rom .\test700000.gba --trainer 1 --out .\out\trainer_1.json
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- update-trainer --rom .\test700000.gba --trainer 1 --ai 2 --slot 1 --slot-level 6 --out-rom .\out\test_trainer_edit.gba --out .\out\trainer_1_update.json
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- update-trainer --rom .\test700000.gba --trainer 1 --data-type 3 --pokemon-count 2 --start 0x08700000 --slot 2 --slot-pokemon 25 --slot-level 10 --slot-item 1 --slot-move1 85 --slot-move2 98 --out-rom .\out\test_trainer_repoint.gba --out .\out\trainer_1_repoint.json
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- import-images --rom .\test700000.gba --target pokemon-sprite --source-dir .\sprites\pokemon --start 0x08F00000 --neworder --out-rom .\out\test_import.gba --log-out .\out\pokemon_import.log.txt
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- import-images --rom .\test700000.gba --target pokemon-sprite --source-dir .\sprites\pokemon_vanilla --start 0x08F00000 --vanilla --out-rom .\out\test_import_vanilla.gba
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- import-images --rom .\test700000.gba --target item-image --source-dir .\sprites\item --start 0x08F10000 --out-rom .\out\test_import.gba
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- import-images --rom .\test700000.gba --target pokemon-icon --source-dir .\sprites\icon --start 0x08F20000 --icon-palette-id 2 --out-rom .\out\test_import.gba
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- import-images --rom .\test700000.gba --target trainer-image --source-dir .\sprites\trainer --start 0x08F30000 --out-rom .\out\test_import.gba
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- export-images --rom .\test700000.gba --target pokemon-sprite --source-dir .\out\pokemon_export --vanilla
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- export-image-sheet --rom .\test700000.gba --target pokemon-sprite --sheet-out .\out\pokemon_front_sheet.png --variant front-normal --vanilla
dotnet run --project .\DarkPochiEditor\DarkPochiEditor\BochiBochiEditor.csproj -- export-image-sheet --rom .\test700000.gba --target pokemon-icon --sheet-out .\out\pokemon_icon_sheet.png --variant frame1 --vanilla
```
