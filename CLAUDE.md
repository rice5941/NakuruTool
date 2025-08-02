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
2. **要求仕様.md** - 機能要件の詳細
3. **詳細設計.md** - アーキテクチャとレイヤー設計
4. **コード規約.md** - コーディングスタイルと規約

### 🏗️ アーキテクチャの理解
- **5層アーキテクチャ**: View → ViewModel → Feature → Model + Adapter
- **レイヤー間ルール**: Model層とViewModel層は直接参照せず、Feature層を介する
- **MVVMパターン**: Livetフレームワークを使用

## 重要な制約と注意事項

### コード規約の重要ポイント
- **コメント**: すべて日本語、VisualStudio標準テンプレート（`///`）使用
- **クラスメンバー順序**: コンストラクタ → プロパティ → 関数 → Dispose → メンバ変数
- **if文**: 1行でも必ず中括弧、`!` の代わりに `== false` 使用
- **null合体演算子**: 複雑な右辺は改行してインデント

## ファイル構造とロケーション

### プロジェクト構成
```
root
├── CollectionConverter\         # コレクションエクスポート機能（現在実装済み）
├── CollectionImporter\         # コレクションインポート機能（現在実装済み）
├── ClaudeCodeDoc\              # 設計ドキュメント類
└── はじめに.md                 # プロジェクト概要
```

### ビルド・実行コマンド
```bash
# 現在のコンソールアプリケーションのビルド
dotnet build CollectionConverter
dotnet build CollectionImporter

# 実行
dotnet run --project CollectionConverter
dotnet run --project CollectionImporter
```

### ビルド・テスト
- 変更後は必ずビルド実行
- エラー発生時は原因特定してから次作業へ
- プロジェクトルートで実行、依存関係確認