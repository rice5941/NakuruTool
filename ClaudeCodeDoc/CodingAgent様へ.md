# 🤖 CodingAgent様へ

このドキュメントは、Nakuru Controller プロジェクトで作業を行うCodingAgentへの重要な指示事項をまとめています。

---

## 🚨 作業実施前の必須確認事項

### 📋 関連ドキュメントの確認
- **[はじめに.md](./はじめに.md)** の **「3.関連ドキュメント」** セクションに記載されているドキュメントを **必ず** 作業前に確認してください
  - ⚠️ ドキュメントは随時更新される可能性があります
  - 最新の情報を確認してから作業を開始してください

### 📁 作業完了時のアーカイブ処理
- **完了した作業指示.md** は以下の手順でアーカイブしてください：
  1. **作業指示履歴フォルダ** (`./作業指示履歴/`) に **コピー** で保管
  2. **日付付きファイル名** で保存（例：`2025-07-26_多言語対応作業_完了.md`）
  3. **作業内容の概要** をファイル先頭に記載
  4. **関連するコミット情報** があれば併記
  5. **重要**: 元の `作業指示.md` は残して次回作業時に再利用する
- **目的**: 
  - 過去の作業内容を検索・参照可能にする
  - プロジェクトの開発履歴を体系的に管理する
  - 類似問題発生時の参考資料として活用する
  - 毎回作業指示をまとめる手間を省く

### 💻 開発環境について
- CLI上でのプロンプト実行時は、以下の点に注意してください：
  - プロジェクトルートディレクトリで実行する
  - 必要な依存関係がインストールされていることを確認する
  - ビルドエラーが発生した場合は、エラー内容を詳細に確認する

---

## 📚 重要なリファレンス

| ドキュメント | 概要 | 確認頻度 |
|------------|------|----------|
| [はじめに.md](./はじめに.md) | プロジェクト全体概要・技術仕様 | 作業開始時・必須 |
| [CLAUDE.md](./CLAUDE.md) | Claude Code用設定・開発ガイドライン | 作業開始時・必須 |
| [Todo.md](./Todo.md) | 作業進捗・タスク管理 | 随時更新 |
| [要求仕様.md](./要求仕様.md) | 機能要件・仕様詳細 | 機能開発時 |
| [過去トラブル.md](./過去トラブル.md) | トラブル事例と対策 | 問題発生時・参考 |

---

## ⚡ 作業効率化のためのヒント

### 1. 🏗️ アーキテクチャの理解
- **5層アーキテクチャ**構成を理解してから作業を開始
- 各層の責務を明確に把握する

### 2. 🗣️ 多言語対応
- 新しいUI要素追加時は、必ず言語切替に対応する
- ハードコードされた文字列は使用しない
- **重要**: `StringFormat` を使用せず、必ず `Converter` を使用する
- **ViewModel側での言語対応**: ViewModelに言語関連プロパティを追加し、PropertyChanged通知を実装する
- **LanguageRepository監視**: `_languageRepository.LanguageData.PropertyChanged` イベントを監視して言語変更時の更新を行う

### 3. 🔄 Converterクラスの使用方針
- **`.NET Framework 4.8` の制約**: `ConverterParameter="{Binding}"` は使用不可
- **解決方法**: 
  - `{Binding .}` でオブジェクト全体をvalueとして渡す
  - Converter内でリフレクションを使用してプロパティを取得
  - 言語切替に対応したConverterを作成する
- **例**: 
  ```csharp
  // ❌ 使用禁止
  StringFormat="現在値: {0} mm"
  ConverterParameter="{Binding KeyNumber}"
  
  // ✅ 推奨
  Converter="{x:Static conv:CommonConverter.StrokeLengthToString}"
  ```

### 4. 🧪 テスト・ビルド
- 変更後は必ずビルドを実行して確認
- エラーが発生した場合は原因を特定してから次の作業に進む

---

## 🔧 Converter実装ガイド

### 📍 Converterファイルの場所
- **ファイルパス**: `Views/Converters/CommonConverter.cs`
- **名前空間**: `NakuruContollerConfiguration.Views.Converters`

### 🏗️ Converter実装パターン

#### 1. 基本的なConverter構造
```csharp
/// <summary>
/// [機能説明]を行うConverter
/// </summary>
public class [機能名]Converter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 言語Repository取得
        var languageRepo = Features.LanguageRepository.Instance;
        
        // 値の変換処理
        // ...
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
```

#### 2. CommonConverterへの登録
```csharp
/// <summary>
/// [機能説明]Converter
/// </summary>
public static readonly IValueConverter [機能名] = new [機能名]Converter();
```

### ⚠️ 重要な注意点
- **必須**: すべてのConverterで `Features.LanguageRepository.Instance` を使用
- **禁止**: ハードコードされた日本語文字列の使用
- **推奨**: リフレクションを使用したプロパティ取得で柔軟性を確保
- **必須**: 日本語コメントの記載

### 🌍 多言語対応の実装例
```csharp
// LanguageStoreに言語キーを追加
{"CurrentValue", "現在値:"}, // 日本語
{"CurrentValue", "Current Value:"}, // 英語

// Converterで言語キーを使用
var currentValueText = languageRepo.GetString("CurrentValue");
return $"{currentValueText} {value:F1} mm";
```

---

## 🔧 多言語対応作業の具体的手順

### 📋 多言語対応が不完全な場合の調査手順
1. **XAMLファイルの確認**: ハードコードされた文字列や `StringFormat` の使用をチェック
2. **ViewModelの確認**: 言語関連プロパティの不足をチェック
3. **LanguageStoreの確認**: 必要な言語キーの不足をチェック
4. **段階的実装**: 小さな単位で変更し、各段階でビルド確認を行う

### 🏗️ ViewModelでの多言語対応実装パターン
```csharp
// コンストラクタ内
_languageRepository = LanguageRepository.Instance;
_languageRepository.LanguageData.PropertyChanged += LanguageData_PropertyChanged;

// 言語関連プロパティ
public string TextSomething
{
    get { return _languageRepository.GetString("Something"); }
}

// 言語変更イベントハンドラ
private void LanguageData_PropertyChanged(object sender, PropertyChangedEventArgs e)
{
    if (e.PropertyName == nameof(Models.LanguageDomain.LanguageData.CurrentLanguageCode))
    {
        RaisePropertyChanged(nameof(TextSomething));
        // 他の言語関連プロパティも同様に通知
    }
}
```

### 🧹 言語キー追加の手順
1. **LanguageStore.cs** の日本語セクションに追加
2. **LanguageStore.cs** の英語セクションに対応する翻訳を追加
3. **ビルドして確認** - エラーがないことを確認

### ⚠️ よくある失敗とその対策
- **PropertyChangedイベントの監視忘れ**: LanguageRepositoryではなく、LanguageDataのPropertyChangedを監視する
- **言語キーの不足**: XAMLで参照される全てのテキストに対応する言語キーが必要
- **Dispose処理の忘れ**: イベント監視の解除を必ずDispose内で実装する
- **重複キーエラー**: LanguageStore.csで同じキーを複数回定義しないよう注意（例：AppTitle重複エラー）
- **ConverterParameter制約**: .NET Framework 4.8では`ConverterParameter="{Binding}"`が使用不可のため、ViewModelに専用プロパティを作成する

---

## 🔗 関連ドキュメントへのリンク

### トラブルシューティング
- **[過去トラブル.md](./過去トラブル.md)**: 過去に発生したトラブル事例と対策の詳細記録

---

## 📞 サポート・質問について

作業中に不明な点や問題が発生した場合は：
1. まず関連ドキュメントを再確認
2. エラーメッセージの詳細を記録
3. 具体的で詳細な質問を行う

---

**最終更新**: 2025年7月26日
**バージョン**: 1.0