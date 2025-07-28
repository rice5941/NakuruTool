# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## 会話ガイドライン

- 常に日本語で会話する

## プロジェクト概要

**はじめに.md**を必ず最初に読んでください。このプロジェクトは音楽ゲームosu!のmaniaモード（7キー）で使用するコレクション管理ツール「osu!7k_collectionManager」の開発プロジェクトです。

### 現在の実装状況
- **現在**: .NET 6.0コンソールアプリケーション（CollectionConverter, CollectionImporter）
- **計画**: WPF + Livet（MVVMパターン）への移行

### 主要技術スタック
- **目標フレームワーク**: WPF + Livet（MVVMパターン）
- **アーキテクチャ**: 5層構成（View, ViewModel, Feature, Model, Adapter）
- **プラットフォーム**: .NET Framework 4.8
- **ライブラリ**: OsuParsers v1.7.2, System.Text.Json

## 必須の作業前確認事項

### 📋 ドキュメント確認順序
1. **はじめに.md** - プロジェクト全体概要
2. **要求仕様.md** - 機能要件の詳細
3. **詳細設計.md** - アーキテクチャとレイヤー設計
4. **コード規約.md** - コーディングスタイルと規約
5. **CodingAgent様へ.md** - 作業指示と重要なガイドライン

### 🏗️ アーキテクチャの理解
- **5層アーキテクチャ**: View → ViewModel → Feature → Model + Adapter
- **レイヤー間ルール**: Model層とViewModel層は直接参照せず、Feature層を介する
- **MVVMパターン**: Livetフレームワークを使用

## 重要な制約と注意事項

### .NET Framework 4.8 制約
- `ConverterParameter="{Binding}"` は使用不可
- 解決方法: `{Binding .}` でオブジェクト全体を渡し、Converter内でリフレクション使用

### 多言語対応の必須実装
- **Language.xml** でキーベース文字列管理
- **StringFormat禁止** - 必ずConverterを使用
- **ViewModel側実装**: `LanguageRepository.Instance` を監視し、PropertyChanged通知
- **言語切替**: 画面右上コンボボックスでリアルタイム切替

### コード規約の重要ポイント
- **コメント**: すべて日本語、VisualStudio標準テンプレート（`///`）使用
- **クラスメンバー順序**: コンストラクタ → プロパティ → 関数 → Dispose → メンバ変数
- **if文**: 1行でも必ず中括弧、`!` の代わりに `== false` 使用
- **null合体演算子**: 複雑な右辺は改行してインデント

## 開発ワークフロー

### Converter実装パターン
```csharp
/// <summary>
/// [機能説明]を行うConverter
/// </summary>
public class [機能名]Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        var languageRepo = Features.LanguageRepository.Instance;
        // 変換処理
    }
}

// CommonConverter.csに登録
public static readonly IValueConverter [機能名] = new [機能名]Converter();
```

### ViewModel多言語対応パターン
```csharp
// コンストラクタ
_languageRepository = LanguageRepository.Instance;
_languageRepository.LanguageData.PropertyChanged += LanguageData_PropertyChanged;

// プロパティ
public string TextSomething => _languageRepository.GetString("Something");

// イベントハンドラ
private void LanguageData_PropertyChanged(object sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(Models.LanguageDomain.LanguageData.CurrentLanguageCode))
        RaisePropertyChanged(nameof(TextSomething));
}
```

## ファイル構造とロケーション

### プロジェクト構成
```
E:\vscode\claudecode02\
├── CollectionConverter\         # コレクションエクスポート機能（現在実装済み）
├── CollectionImporter\         # コレクションインポート機能（現在実装済み）
├── osu!7k_collectionManger\   # 将来のGUIアプリケーション（未実装）
├── ClaudeCodeDoc\              # 設計ドキュメント類
└── はじめに.md                 # プロジェクト概要
```

### 重要ファイル
- **Views/Converters/CommonConverter.cs** - 共通Converter集約（将来実装）
- **Language.xml** - 多言語文字列管理（将来実装）
- **ClaudeCodeDoc/Todo.md** - 作業進捗管理（随時更新必須）

### ビルド・実行コマンド
```bash
# 現在のコンソールアプリケーションのビルド
dotnet build CollectionConverter
dotnet build CollectionImporter

# 実行
dotnet run --project CollectionConverter
dotnet run --project CollectionImporter
```

### 作業完了時の処理
- **ClaudeCodeDoc/Todo.md** を作業進捗に応じて更新
- 完了した作業指示は `./作業指示履歴/` にアーカイブ

## よくある問題と対策

### 多言語対応の失敗パターン
- PropertyChangedイベント監視忘れ（LanguageDataを監視、LanguageRepositoryではない）
- 言語キー不足
- Dispose処理でのイベント解除忘れ
- 重複キーエラー（LanguageStore.cs）

### ビルド・テスト
- 変更後は必ずビルド実行
- エラー発生時は原因特定してから次作業へ
- プロジェクトルートで実行、依存関係確認