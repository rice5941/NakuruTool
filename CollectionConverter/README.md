# osu! Collection Converter

osu!のcollection.dbファイルを読み込み、JSON形式に変換するコンソールアプリケーションです。

---

## 📋 ユーザー向けガイド

### ✨ 主な機能
- osu!コレクションを読み込んでJSON形式に変換
- 全コレクションまたは特定のコレクションのみ選択可能
- 各コレクションを個別ファイルとして出力

### 🚀 クイックスタート

1. **プロジェクトをビルド**
```bash
dotnet build
```

2. **設定ファイルを編集**（初回実行時に自動作成）
```json
{
  "OsuFolderPath": "C:/Users/YourUsername/AppData/Local/osu!",
  "OutputFolder": "output",
  "TargetCollections": null
}
```

3. **実行**

**すべてのコレクションを変換:**
```bash
dotnet run
```
→ 各コレクションが`output`フォルダ内に個別の `コレクション名.json` ファイルとして出力

**指定したコレクションのみ変換:**
```bash
dotnet run "7K_Ranked"                           # 1個指定
dotnet run "7K_Ranked" "LN本質" "とても良い譜面"  # 複数指定
```
→ 指定したコレクションが個別ファイルとして出力

### 🔍 コレクション検索機能

賢い検索で目的のコレクションを簡単に見つけられます：

1. **完全一致** → **大文字小文字無視** → **部分一致** の順で検索
2. 部分一致で複数見つかった場合は候補を表示
3. 見つからない場合は利用可能なコレクション一覧を表示

**使用例:**
```bash
dotnet run "7K_Ranked"                           # → output/7K_Ranked.json
dotnet run "7k_ranked"                           # 大小文字無視 → output/7K_Ranked.json  
dotnet run "LN"                                  # 部分一致（候補表示）
dotnet run "とても良い譜面"                      # 日本語対応 → output/とても良い譜面.json
dotnet run "7K_Ranked" "LN本質" "とても良い譜面"  # → 各ファイル出力
```

### 📁 出力形式

```json
[
  {
    "name": "7K_Ranked",
    "beatmaps": [
      {
        "md5": "abc123...",
        "title": "楽曲タイトル",
        "artist": "アーティスト名",
        "version": "難易度名",
        "creator": "マッパー名",
        "cs": 7.0,
        "mode": "Mania",
        "status": "Ranked",
        "beatmapset_id": 123456,
        "beatmap_id": 789012
      }
    ]
  }
]
```

### ⚙️ 設定

**config.json**
- `OsuFolderPath`: osu!フォルダのパス（collection.db, osu!.dbがある場所）
- `OutputFolder`: 出力フォルダのパス（デフォルト: "output"）
- `TargetCollections`: コレクション指定（配列形式）

**コレクション指定の動作:**
- **引数あり**: 引数で指定されたコレクションのみ処理（config.jsonの設定は無視）
- **引数なし**: config.jsonの設定に従って処理
  1. `TargetCollections` 指定
  2. 全コレクション処理 （設定なし）

**設定例:**
```json
{
  "TargetCollections": ["7K_Ranked", "LN本質", "とても良い譜面"]
}
```

### 🎯 パフォーマンス

- **処理速度**: 約2秒で128,000ビートマップを処理
- **メモリ効率**: 従来比84%のメモリ削減
- **並列処理**: マルチコアCPUを自動活用

---

## 🛠️ 開発者向け情報

### 🏗️ アーキテクチャ

#### メモリ最適化戦略
軽量化処理により劇的なメモリ削減を実現：

**Before (DbBeatmap)**: 377MB
```csharp
// 重いデータ（JSON出力には不要）
- StandardStarRating (Dictionary<Mods, double>)
- TaikoStarRating, CatchStarRating, ManiaStarRating
- TimingPoints (List<TimingPoint>)
- HitObjects
- 詳細なメタデータ
```

**After (OptimizedBeatmapInfo)**: 58MB (-84%)
```csharp
// 必要最小限のデータ
- Title, Artist, Difficulty, Creator (string)
- CircleSize (float)
- Ruleset (enum)
- RankedStatus (object)
- BeatmapSetId, BeatmapId (int)
```

#### 並列処理設計
- `Task.Run` + `Parallel.ForEach` による多層並列化
- `ConcurrentDictionary` での安全な並行アクセス
- `Environment.ProcessorCount` による自動スケーリング

#### ストリーミング処理
- `Utf8JsonWriter` による低メモリJSON出力
- 1,000件バッチでのメモリ制御
- 定期的GCによるメモリ圧迫防止

### 📊 技術仕様

**依存関係:**
- **.NET 6.0**: 実行環境
- **OsuParsers 1.7.2**: osu!データベース解析
- **System.Text.Json 8.0.0**: JSON処理

**対応フォーマット:**
- collection.db（ULEB128 + UTF-8）
- osu!.db（OsuParsersライブラリ経由）

**ファイル構成:**
```
CollectionConverter/
├── CollectionConverter.cs       # メインロジック
├── CollectionConverter.csproj   # プロジェクト設定
├── config.json                  # 設定ファイル
└── README.md                    # ドキュメント
```

### 🔧 主要メソッド

#### コア処理
- `ReadCollectionDb()`: collection.dbバイナリ解析
- `ReadOsuDbWithOsuParsersParallel()`: osu!.db並列読み込み
- `ConvertToOptimizedBeatmapInfos()`: メモリ最適化変換
- `WriteToJsonParallel()`: ストリーミングJSON出力

#### ユーティリティ
- `FilterCollections()`: インテリジェント検索
- `SanitizeFileName()`: 安全なファイル名生成
- `ReadOsuString()`: ULEB128文字列デコード（ReadOnlySpan最適化）

### 📈 パフォーマンス詳細

**メモリ最適化版（v2.0）**
- 処理時間: 2.22秒（128,941ビートマップ）
- メモリ使用量: 58MB（377MB→58MB, 84%削減）
- 内訳:
  - osu!.db処理: 1,183ms
  - 軽量化変換: 51ms
  - ストリーミング出力: 393ms

**軽量化のメリット:**
1. **スケーラビリティ**: より大規模なデータセットに対応
2. **GC効率**: ガベージコレクション負荷軽減
3. **キャッシュ効率**: CPUキャッシュヒット率向上
4. **並列性能**: メモリ競合の軽減

### 🔒 セキュリティ考慮事項

- ファイル名サニタイゼーション（パストラバーサル対策）
- 無効文字の安全な置換
- ファイル長制限（255文字制限）
- エラー時の適切な終了処理

### 🧪 テスト観点

- 大規模データセット（100,000+ビートマップ）
- 特殊文字を含むコレクション名
- メモリ制約環境での動作
- 並列処理の安全性
- ファイル名の安全性