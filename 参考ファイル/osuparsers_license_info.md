# OsuParsers ライブラリ - ライセンスと詳細情報

## ライセンス情報

**OsuParsersはMITライセンス**で公開されており、Copyright 2019年の表記があります。MITライセンスは非常に自由度の高いライセンスで、商用・非商用問わず自由に使用、改変、再配布が可能です。

## ライブラリ基本情報

- **開発者**: mrflashstudio（Михаил Черепанов / Mikhail Cherepanov）
- **最新バージョン**: 1.7.2 （2025年1月11日リリース）
- **リポジトリ**: https://github.com/mrflashstudio/OsuParsers
- **説明**: osu!に関連するファイルのパース/書き込み用のC#ライブラリ

## 対応フレームワーク

**.NET Standard 2.0**をターゲットとしており、以下のフレームワークと互換性があります：

- .NET Framework 4.6.1以上
- .NET Core 2.0以上  
- .NET 5以上
- Unity（.NET Standard 2.0対応版）

## 依存関係

**唯一の外部依存**: System.Numerics.Vectors (>= 4.5.0)

このライブラリは数値計算用のハードウェア最適化された型を提供し、高性能処理やグラフィックス用途に使用されます。

## インストール方法

### NuGetパッケージマネージャー
```
Install-Package OsuParsers
```

### .NET CLI
```
dotnet add package OsuParsers
```

### PackageReference
```xml
<PackageReference Include="OsuParsers" Version="1.7.2" />
```

## 対応ファイル形式

OsuParsersは以下のosu!ファイルの読み書きに対応しています：

- **ビートマップ**: `.osu`ファイル
- **ストーリーボード**: `.osb`ファイル  
- **リプレイ**: `.osr`ファイル
- **データベース**: `osu!.db`, `collection.db`, `scores.db`, `presence.db`

## 使用例

### collection.dbの読み込み
```csharp
using OsuParsers.Decoders;
using OsuParsers.Database;

// collection.dbの読み込み
CollectionDatabase collectionDb = DatabaseDecoder.DecodeCollection(@"collection.db");
Console.WriteLine($"コレクション数: {collectionDb.CollectionCount}");
```

### ビートマップの読み込み
```csharp
using OsuParsers.Beatmaps;
using OsuParsers.Decoders;

// ビートマップの読み込み
Beatmap beatmap = BeatmapDecoder.Decode(@"beatmap.osu");
Console.WriteLine($"タイトル: {beatmap.MetadataSection.TitleUnicode}");
```

### リプレイの読み込み
```csharp
using OsuParsers.Decoders;
using OsuParsers.Replays;

// リプレイの読み込み
Replay replay = ReplayDecoder.Decode(@"replay.osr");
Console.WriteLine($"プレイヤー名: {replay.PlayerName}");
```

### データベースの書き込み
```csharp
using OsuParsers.Database;
using OsuParsers.Decoders;

// osu!データベースの読み込みと書き込み
OsuDatabase db = DatabaseDecoder.DecodeOsu(@"osu!.db");
db.Permissions = Permissions.Supporter;
db.Save(@"modified_osu!.db");
```

## ライセンス条件（MIT）

MITライセンスの主な条件：

### 許可される行為
- ✅ **商用利用可能**
- ✅ **改変可能**  
- ✅ **再配布可能**
- ✅ **プライベート利用可能**
- ✅ **サブライセンス可能**

### 必須条件
- ⚠️ **著作権表示とライセンス条項の保持が必要**

### 制限事項
- ❌ **保証なし**（無保証）
- ❌ **責任制限**

## 開発とコミュニティ

- 開発者は積極的にメンテナンスを行っており、"何か問題があれば気軽に報告してほしい"とコメントしています
- NuGetでの総ダウンロード数は19,000回を超えており、osu!開発コミュニティで広く使用されています
- GitHubでのスター数: 78個、フォーク数: 17個（2025年1月時点）

## 最新更新情報（v1.7.2）

- osu!ビルド20250107以降で作成されたデータベースのデコード時のクラッシュを修正
- osu!ビルド20250107以降のデータベースエンコード時の不正な出力を修正

## 注意点

### System.Numerics.Vectorsの互換性
System.Numerics.Vectorsライブラリには.NET Frameworkでの互換性問題が報告されている場合があります。特に古いバージョンの.NET Frameworkを使用する場合は、バインディングリダイレクトの設定が必要になることがあります。

### バインディングリダイレクトの例
```xml
<runtime>
  <assemblyBinding xmlns="urn:schemas-microsoft-com:asm.v1">
    <dependentAssembly>
      <assemblyIdentity name="System.Numerics.Vectors" 
                        publicKeyToken="b03f5f7f11d50a3a" 
                        culture="neutral" />
      <bindingRedirect oldVersion="0.0.0.0-4.5.0.0" newVersion="4.5.0.0" />
    </dependentAssembly>
  </assemblyBinding>
</runtime>
```

## 関連プロジェクト

OsuParsersを使用しているプロジェクト例：
- ManiaToIntralism: mania譜面をIntralist/Intralismに変換するツール

## まとめ

このライブラリは、osu!関連のアプリケーション開発において非常に有用で、MITライセンスにより商用プロジェクトでも安心して使用できます。豊富な機能と安定したAPIにより、osu!のファイル操作を簡単に実装することが可能です。

---

**参考リンク**:
- [GitHub リポジトリ](https://github.com/mrflashstudio/OsuParsers)
- [NuGet パッケージ](https://www.nuget.org/packages/OsuParsers)
- [ドキュメント](https://github.com/mrflashstudio/OsuParsers/blob/master/docs)