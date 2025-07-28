# osu! Collection Importer

JSONファイルからosu!のcollection.dbファイルにコレクションをインポートするツールです。

## 機能

- JSON形式のコレクションファイルを読み込み
- 既存のcollection.dbを自動バックアップ
- MD5ハッシュの有効性を検証
- 複数のJSONファイルを一括処理
- osu!データベースとの整合性チェック

## 使用方法

### 基本的な使用方法

1. `config.json` でosu!フォルダのパスを設定
2. `input` フォルダにJSONファイルを配置
3. `CollectionImporter.exe` を実行

### コマンドライン実行

```bash
CollectionImporter.exe
```

### 設定ファイル (config.json)

```json
{
  "OsuFolderPath": "C:/Users/YourUsername/AppData/Local/osu!",
  "InputFolder": "input"
}
```

- `OsuFolderPath`: osu!がインストールされているフォルダのパス
- `InputFolder`: JSONファイルが配置されているフォルダ

### JSON形式

#### 単一コレクション
```json
{
  "name": "My Collection",
  "beatmaps": [
    {
      "artist": "Artist Name",
      "title": "Song Title",
      "version": "Difficulty Name",
      "creator": "Mapper Name",
      "md5_hash": "abcdef1234567890abcdef1234567890"
    }
  ]
}
```

#### 複数コレクション
```json
[
  {
    "name": "Collection 1",
    "beatmaps": [...]
  },
  {
    "name": "Collection 2", 
    "beatmaps": [...]
  }
]
```

## 安全機能

- **自動バックアップ**: 既存のcollection.dbをbackupフォルダに保存
- **MD5検証**: osu!.dbと照合して有効なビートマップのみをインポート
- **エラーハンドリング**: 無効なJSONファイルをスキップして処理継続

## 注意事項

- osu!を終了してから実行してください
- バックアップファイルは定期的に整理してください
- JSONファイルのMD5ハッシュは正確である必要があります

## 依存関係

- .NET 6.0
- OsuParsers v1.7.2 (MIT License)
- System.Text.Json v8.0.5 (MIT License)

## ライセンス

このプロジェクトは参考目的で作成されています。使用している外部ライブラリのライセンスに従ってください。