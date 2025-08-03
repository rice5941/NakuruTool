# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 会話ガイドライン

- 思考は英語で行うが回答は日本語で

## プロジェクト概要

このプロジェクトは音楽ゲームosu!のmaniaモード（7キー）で使用するコレクション管理ツール「NakuruTool」の開発プロジェクトです。

### 現在の実装状況
- **現在**: .NET 6.0コンソールアプリケーション（CollectionConverter, CollectionImporter）
- **計画**: WPF + Livet（MVVMパターン）への移行

### 主要技術スタック
- **目標フレームワーク**: WPF + Livet（MVVMパターン）
- **プラットフォーム**: .NET 6.0
- **ライブラリ**: OsuParsers v1.7.2, System.Text.Json

## 必須の作業前確認事項

### 📋 ドキュメント確認順序
1. **はじめに.md** - プロジェクト全体概要

### 🏗️ アーキテクチャの理解
- **MVVMパターン**: Livetフレームワークを使用

### ビルド・テスト
- 変更後は必ずビルド実行
- エラー発生時は原因特定してから次作業へ
- プロジェクトルートで実行、依存関係確認

## ファイル構造とロケーション

### プロジェクト構成
```
root
├── CollectionConverter\        # コレクションエクスポート機能（現在実装済み）
├── CollectionImporter\         # コレクションインポート機能（現在実装済み）
├── NakuruTool\                 # NakuruTool（開発中）
├── ClaudeCodeDoc\              # 設計ドキュメント類
└── はじめに.md                  # プロジェクト概要
```

### ビルド・実行コマンド
```bash
# NakuruTool WPFアプリケーションのビルド
cd NakuruTool && dotnet build

# 現在のコンソールアプリケーションのビルド
dotnet build CollectionConverter
dotnet build CollectionImporter

# 実行
dotnet run --project CollectionConverter
dotnet run --project CollectionImporter
```

## 🎯 開発効率化ガイドライン

### 必須の事前確認事項
1. **コーディング規約の確認**
   - `ClaudeCodeDoc/coding_rule/common.md` - 基本規約
   - `ClaudeCodeDoc/coding_rule/view.md` - View層規約
   - 否定演算子は `!` ではなく `== false` を使用
   - 単行if文でも必ず `{}` を使用

2. **詳細設計の確認**
   - `ClaudeCodeDoc/詳細設計.md` - 全体アーキテクチャ
   - `ClaudeCodeDoc/detail_design/` - 各層の詳細設計
   - MVVMパターンでの責務分離を厳守

### 🏗️ アーキテクチャパターン

#### テーマシステム
- **場所**: `NakuruTool/Models/Theme/`
- **パターン**: Domain-Store-Enum構成
- **拡張時**: ThemeType enumに追加 → Theme.cs で分岐処理 → XAMLリソース作成

#### 多言語対応
- **リソース**: `NakuruTool/Languages/Language.xml`
- **パターン**: DynamicResourceバインディング
- **新規追加時**: Language.xmlに10言語分エントリ追加必須

#### フィルタリング機能
- **必須使用**: `ToLowerInvariant()` で地域非依存比較
- **リアルタイム**: `UpdateSourceTrigger=PropertyChanged`
- **UI**: 虫眼鏡アイコン + プレースホルダーテキスト

#### プロパティ変更通知システム
- **統一方式**: 全てのViewModel/ModelでLivet.RaisePropertyChangedIfSet使用
- **禁止事項**: SetProperty、独自NotificationBase使用禁止（削除済み）

**基底クラス選択基準**:
- ViewModel: `Livet.ViewModel`継承
- Model: `Livet.NotificationObject`継承

**標準プロパティパターン**:
```csharp
// 基本パターン
set { RaisePropertyChangedIfSet(ref _field, value); }

// 関連プロパティ通知
set { RaisePropertyChangedIfSet(ref _field, value, nameof(RelatedProperty)); }

// 追加処理付き
if (RaisePropertyChangedIfSet(ref _field, value))
{
    UpdateRelatedState();
}
```

#### ViewModel間通信パターン
**重要**: PropertyChanged統一方式を採用

**基本方針**:
- **PropertyChanged統一**: 全ての通信（View↔ViewModel、ViewModel↔ViewModel）でPropertyChanged使用
- **一貫性重視**: 複数の通信方式による混乱を回避
- **WPF標準準拠**: MVVMパターンの標準実装に従う

**推奨実装パターン**:

1. **基本監視パターン**:
```csharp
// 受信側: シンプルな監視
private void OnOperationPanelPropertyChanged(object sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(OperationPanelViewModel.IsLoading))
    {
        UpdateLoadingState();
    }
}
```

2. **複数プロパティ監視パターン**:
```csharp
// 効率的な複数プロパティ監視
private void OnOperationPanelPropertyChanged(object sender, PropertyChangedEventArgs e)
{
    switch (e.PropertyName)
    {
        case nameof(OperationPanelViewModel.IsLoading):
        case nameof(OperationPanelViewModel.StatusMessage):
            UpdateStatus();
            break;
    }
}
```

3. **配列ベース監視パターン**:
```csharp
// 保守性重視の監視方式
private readonly string[] _statusProperties = {
    nameof(OperationPanelViewModel.IsLoading),
    nameof(OperationPanelViewModel.StatusMessage)
};

private void OnOperationPanelPropertyChanged(object sender, PropertyChangedEventArgs e)
{
    if (_statusProperties.Contains(e.PropertyName))
    {
        UpdateStatus();
    }
}
```

**設計判断基準**:
- **全ての通信**: PropertyChanged使用
- **型安全性**: nameof()でコンパイル時型チェック
- **一貫性**: 同じパターンの統一使用

**禁止事項**:
- カスタムEventArgsの作成（一貫性を損なう）
- 文字列リテラルでのプロパティ名指定
- PropertyChanged以外の通信方式併用

#### PropertyChangedパフォーマンス最適化パターン
**重要**: 大量データ処理時のO(n²)問題回避

**問題となるパターン**:
```csharp
// ❌ 悪い例: O(n²)になる実装
private void SelectAllItems()
{
    foreach (var item in Items)
    {
        item.IsSelected = true; // 毎回PropertyChangedが発火
        // → 毎回SelectedCountが再計算される（全要素をチェック）
    }
}

public int SelectedCount => Items?.Count(x => x.IsSelected) ?? 0; // 計算プロパティ
```

**最適化パターン**:
```csharp
// ✅ 良い例: O(n)に最適化
private void SelectAllItems()
{
    if (Items == null) return;
    
    // パフォーマンス最適化: イベント監視を一時停止
    UnsubscribeFromItemEvents();
    
    try
    {
        // 全アイテムを選択（PropertyChangedは発火するが監視されない）
        foreach (var item in Items)
        {
            item.IsSelected = true;
        }
    }
    finally
    {
        // イベント監視を再開
        SubscribeToItemEvents();
        
        // 一括で更新通知（O(1)の処理）
        RaisePropertyChanged(nameof(SelectedCount));
        RaisePropertyChanged(nameof(SelectedCountText));
        UpdateCommandStates();
    }
}
```

**パフォーマンス改善効果**:
- 1000件の場合: 100万回処理 → 1000回処理（99%削減）
- UIフリーズの解消
- メモリ使用量の削減

**適用対象**:
- 全選択/全選択解除処理
- 大量データの一括更新
- 計算プロパティに依存する繰り返し処理

#### PLINQパフォーマンス最適化

**問題**: 初回エクスポート時の15秒遅延（JITコンパイル）
- 原因: System.Linq.Parallel.dll の遅延読み込み + 初回JITコンパイル
- 影響: 初回のみ異常に長い処理時間（40件で15秒）

**解決策**: アプリケーション起動時PLINQウォームアップ

**実装場所**: `App.xaml.cs - Application_Startup()`
```csharp
private void WarmupPlinq()
{
    // ExportStoreと同じPLINQパターンで事前JITコンパイル
    var dummyData = Enumerable.Range(1, 10000).ToList();
    var result = new ConcurrentDictionary<int, string>();

    Parallel.ForEach(dummyData, new ParallelOptions
    {
        MaxDegreeOfParallelism = Environment.ProcessorCount
    }, item => result.TryAdd(item, $"dummy_{item}"));

    var parallelResult = dummyData.AsParallel()
        .WithDegreeOfParallelism(Environment.ProcessorCount)
        .Select(x => new { id = x, value = $"parallel_{x}" })
        .ToArray();
}
```

**効果**:
- **劇的改善**: 初回エクスポート 15秒 → 1.3秒（92%削減）
- **起動コスト**: 116ms（軽微）
- **対象データ**: 128,941曲でも135ms高速処理

**検証結果**:
```
[PLINQウォームアップ] 処理時間: 116ms
[ExportStore.LoadBeatmapDatabase] Parallel.ForEach: 135ms（従来12秒）
System.Linq.Parallel.dll読み込みログなし（事前解決済み）
```

**将来の改善案**:
```csharp
// 非同期ウォームアップ（起動時間への影響完全排除）
Task.Run(() => WarmupPlinq()); // バックグラウンド実行
```

### 🔧 開発ワークフロー

#### 新機能開発時
1. **要件定義** → TodoWrite ツールでタスク管理
2. **設計確認** → 詳細設計ドキュメント参照
3. **実装** → コーディング規約遵守
4. **ビルド確認** → `cd NakuruTool && dotnet build`
5. **品質チェック** → code-quality-reviewer エージェント使用

#### コード品質管理
- **必須チェック**: 実装完了後にcode-quality-reviewerエージェント実行
- **エラーハンドリング**: try-catch + Debug.WriteLine + ユーザー向けメッセージ
- **プロパティ変更通知**: 全てのViewModel/ModelでLivet.RaisePropertyChangedIfSet統一使用

#### プロパティ実装時の必須チェック
1. **基底クラス確認**: ViewModel=`Livet.ViewModel`, Model=`Livet.NotificationObject`
2. **変更通知方式**: `RaisePropertyChangedIfSet`のみ使用
3. **関連プロパティ**: 計算プロパティがある場合は`nameof()`で明示的通知
4. **戻り値活用**: 追加処理が必要な場合はif文で戻り値チェック

### 📁 ファイル配置規則

#### 新しいView追加時
```
NakuruTool/Views/[機能名]/
├── [機能名]Control.xaml          # UserControl
├── [機能名]Control.xaml.cs       # コードビハインド（最小限）
└── [機能名]Window.xaml           # Window（必要時）
```

#### 新しいViewModel追加時
```
NakuruTool/ViewModels/[機能名]/
└── [機能名]ViewModel.cs          # Livet.ViewModel継承
```

#### 新しいModel追加時
```
NakuruTool/Models/[機能名]/
├── [機能名]Domain.cs             # データ実体管理
├── [機能名]Store.cs              # データ操作
├── [機能名].cs                   # データクラス（Livet.NotificationObject継承）
└── [機能名]Type.cs               # 列挙型（必要時）
```

### 🏛️ 責務分離原則

#### ViewModel責務分離
- **MainWindowViewModel**:
  - 責務: コレクション表示、UI制御、言語設定
  - 禁止事項: OsuFolderPath管理、重複するステータス処理

- **OperationPanelViewModel**:
  - 責務: OsuFolderPath管理、コレクション読み込み、操作パネル状態管理
  - 重要: ConfigManager.OsuFolderPathChangedイベントの唯一の処理場所

#### 重複処理の回避
- 同一機能を複数ViewModelで実装禁止
- データフローは単方向: ConfigManager → OperationPanelViewModel → MainWindowViewModel

### 🎨 UIコンポーネント標準パターン

#### テーマ対応コントロール
- 必ず `{DynamicResource [リソース名]}` を使用
- 新しいテーマ追加時は3つのXAMLファイル更新必須

### 🛠️ トラブルシューティング

#### よくある問題と解決方法
1. **テーマが保存されない**
   - ConfigManager.UpdateThemeType() が呼ばれているか確認
   - ThemeDomain.SetTheme() を使用しているか確認

2. **フィルタが動作しない**
   - UpdateSourceTrigger=PropertyChanged 設定確認
   - ToLowerInvariant() 使用確認
   - FilteredCollection への適切なバインディング確認

3. **多言語表示されない**
   - DynamicResource 使用確認
   - Language.xml にエントリ存在確認
   - LanguageManager 初期化確認

### 📊 品質基準
- **コード品質スコア**: 8.5/10 以上を維持
- **ビルド**: エラー・警告なしで完了
- **設計準拠**: MVVMパターンとLivet使用規則の遵守
- **多言語対応**: 最低10言語での対応完了

#### アーキテクチャ準拠チェックリスト
- [ ] 全プロパティでRaisePropertyChangedIfSet使用
- [ ] 適切な基底クラス選択（ViewModel/Model）
- [ ] 責務重複なし
- [ ] ビルドエラー・警告なし