# NotificationBase設計書

## 概要

`NotificationBase`クラスは、WPF/Livet MVVMアプリケーションにおけるプロパティ変更通知機能の効率化を目的とした基底クラスです。LivetフレームワークのNotificationObjectを継承し、汎用的なSetPropertyメソッドを提供することで、ViewModelとModelクラスでの実装の一貫性と効率性を向上させます。

## 設計方針

### 1. Livetとの統合
- LivetのNotificationObjectを継承し、既存のフレームワーク機能と完全互換
- RaisePropertyChangedメソッドを活用した通知システム
- 既存のLivetプロジェクトへの段階的導入が可能

### 2. パフォーマンス最適化
- EqualityComparer<T>.Defaultによる効率的な値比較
- 値が変更されない場合はイベント発生を抑制
- 最小限のメモリ割り当てとCPU使用量

### 3. 開発効率の向上
- CallerMemberName属性による自動プロパティ名取得
- 複数のSetPropertyメソッドオーバーロードによる柔軟な実装
- 関連プロパティの自動更新通知機能

## 実装構造

### ファイル構成
```
NakuruTool/
├── Models/
│   ├── NotificationBase.cs           # メイン基底クラス
│   ├── Examples/
│   │   └── NotificationBaseUsageExample.cs  # 使用例
│   └── Theme/
│       └── Theme.cs                  # 実際の適用例
```

### 基底クラス階層
```
NotificationObject (Livet)
└── NotificationBase (本実装)
    ├── Theme
    ├── その他のModelクラス
    └── 必要に応じてViewModelでも使用可能
```

## SetPropertyメソッドの詳細

### 1. 基本的なSetProperty
```csharp
protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
```

**用途**: 最も基本的なプロパティ設定
**特徴**: 
- 値の変更確認とPropertyChangedイベントの自動発生
- 戻り値でプロパティが実際に変更されたかを確認可能

**使用例**:
```csharp
private string _name;
public string Name
{
    get { return _name; }
    set { SetProperty(ref _name, value); }
}
```

### 2. 変更前後処理付きSetProperty
```csharp
protected virtual bool SetProperty<T>(ref T field, T value, 
    Action<T, T> onChanging, 
    Action<T, T> onChanged = null, 
    [CallerMemberName] string propertyName = null)
```

**用途**: 値変更時に追加の処理が必要な場合
**特徴**: 
- onChanging: 値変更前に実行（バリデーション等）
- onChanged: 値変更後に実行（後処理等）
- 変更前後の値を引数として取得可能

**使用例**:
```csharp
private int _age;
public int Age
{
    get { return _age; }
    set 
    { 
        SetProperty(ref _age, value, 
            // 変更前処理：値の検証
            (oldValue, newValue) => 
            {
                if (newValue < 0 || newValue > 150)
                {
                    throw new ArgumentOutOfRangeException();
                }
            },
            // 変更後処理：関連プロパティの更新通知
            (oldValue, newValue) => 
            {
                RaisePropertyChanged(nameof(IsAdult));
            }
        );
    }
}
```

### 3. 関連プロパティ同時更新SetProperty
```csharp
protected virtual bool SetProperty<T>(ref T field, T value, 
    string[] dependentProperties, 
    [CallerMemberName] string propertyName = null)
```

**用途**: 一つのプロパティの変更で複数の関連プロパティも更新が必要な場合
**特徴**: 
- 関連プロパティの配列を指定
- 一度の変更で複数のPropertyChangedイベントを効率的に発生

**使用例**:
```csharp
private bool _isDarkTheme;
public bool IsDarkTheme 
{ 
    get { return _isDarkTheme; }
    set { SetProperty(ref _isDarkTheme, value, new string[] { nameof(ThemeName), nameof(ThemeResourceUri) }); }
}
```

## 実装ベストプラクティス

### 1. プロパティの命名規則
- プロパティ名: PascalCase
- バッキングフィールド: アンダースコア + camelCase

```csharp
private string _userName;  // バッキングフィールド
public string UserName     // プロパティ
{
    get { return _userName; }
    set { SetProperty(ref _userName, value); }
}
```

### 2. 読み取り専用プロパティとの連携
関連する読み取り専用プロパティがある場合は、依存関係を明確にする：

```csharp
// 基本プロパティ
public bool IsActive
{
    get { return _isActive; }
    set { SetProperty(ref _isActive, value, new string[] { nameof(StatusText) }); }
}

// 依存プロパティ（読み取り専用）
public string StatusText
{
    get { return IsActive ? "アクティブ" : "非アクティブ"; }
}
```

### 3. コレクションプロパティの処理
ObservableCollection等のコレクションでは、コレクション自体の変更と内容の変更を区別：

```csharp
private ObservableCollection<string> _items;
public ObservableCollection<string> Items
{
    get { return _items; }
    set { SetProperty(ref _items, value, new string[] { nameof(ItemCount) }); }
}

// コレクション内容変更時は手動で通知
public void AddItem(string item)
{
    _items.Add(item);
    RaisePropertyChanged(nameof(ItemCount));
}
```

### 4. ViewModelでの使用
ViewModelクラスでも同様のパターンで使用可能（Livet.ViewModelを継承している場合）：

```csharp
public class SampleViewModel : ViewModel
{
    private string _title;
    public string Title
    {
        get { return _title; }
        set { SetProperty(ref _title, value); }
    }

    // ViewModelのSetPropertyメソッドを実装（簡易版）
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
            return false;

        field = value;
        RaisePropertyChanged(propertyName);
        return true;
    }
}
```

## パフォーマンス考慮事項

### 1. メモリ効率
- バッキングフィールドは必要最小限に留める
- 大きなオブジェクトの不要なコピーを避ける
- EqualityComparer<T>.Defaultによる効率的な比較

### 2. 通知頻度の最適化
- 値が実際に変更された場合のみイベント発生
- 関連プロパティの一括更新による通知回数削減
- UI更新が不要な内部状態変更では通知を抑制

### 3. スレッドセーフティ
- 基本実装はUIスレッド専用
- マルチスレッド環境では適切なディスパッチャーの使用が必要

## 拡張可能性

### 1. カスタムバリデーション
変更前処理を活用したバリデーション機能の実装：

```csharp
protected bool SetPropertyWithValidation<T>(ref T field, T value, 
    Func<T, bool> validator, 
    string errorMessage = null,
    [CallerMemberName] string propertyName = null)
{
    return SetProperty(ref field, value,
        (oldValue, newValue) =>
        {
            if (!validator(newValue))
                throw new ArgumentException(errorMessage ?? "Invalid value");
        },
        propertyName: propertyName);
}
```

### 2. 変更履歴の追跡
変更前後処理を活用した変更履歴機能：

```csharp
private readonly List<PropertyChange> _changeHistory = new();

protected bool SetPropertyWithHistory<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
{
    return SetProperty(ref field, value,
        null,
        (oldValue, newValue) =>
        {
            _changeHistory.Add(new PropertyChange(propertyName, oldValue, newValue, DateTime.Now));
        },
        propertyName);
}
```

## 移行ガイド

### 既存プロジェクトへの導入手順

1. **NotificationBase.csの追加**
   - Models/NotificationBase.csファイルを追加

2. **段階的移行**
   - 新しいModelクラスからNotificationBaseを継承
   - 既存クラスは必要に応じて順次移行

3. **Theme.csの更新**
   - 既存のThemeクラスを修正例として活用
   - プロパティの依存関係を整理

4. **テストとビルド確認**
   - dotnet buildでコンパイルエラーがないことを確認
   - 既存機能に影響がないことを検証

## トラブルシューティング

### よくある問題と解決方法

1. **コンパイルエラー: CallerMemberNameが見つからない**
   ```csharp
   using System.Runtime.CompilerServices;
   ```
   を追加

2. **PropertyChangedイベントが発生しない**
   - バッキングフィールドとプロパティ名の対応確認
   - RaisePropertyChangedが正しく呼ばれているか確認

3. **無限再帰の発生（RaisePropertyChanged関連）** - 🔧**修正済み**
   - **問題**: 基底クラス（Livet.NotificationObject）の`RaisePropertyChanged(string)`メソッドとカスタム実装の`RaisePropertyChanged(params string[])`メソッドが名前競合を起こし無限再帰が発生
   - **解決策**: カスタムメソッド名を`RaiseMultiplePropertyChanged`に変更して競合回避
   - **使用方法**: 複数プロパティの一括通知には`RaiseMultiplePropertyChanged(nameof(Prop1), nameof(Prop2))`を使用
   - SetPropertyの戻り値を確認し、変更が実際に行われた場合のみ後続処理を実行

4. **パフォーマンスの低下**
   - 関連プロパティの通知が過剰でないか確認
   - 重い処理をSetPropertyの後続処理に含めていないか確認

## 修正履歴

### 2025-08-02: RaisePropertyChanged無限再帰問題の修正
- **問題**: カスタム実装の`RaisePropertyChanged(params string[])`メソッドがLivet基底クラスのメソッドと名前競合し無限再帰が発生
- **修正**: メソッド名を`RaiseMultiplePropertyChanged`に変更して競合を回避
- **影響**: 既存の単一プロパティ通知機能は影響なし、複数プロパティ通知も正常動作
- **検証**: ビルド成功、デバッグ時のスタックトレースエラー解消

## まとめ

NotificationBaseクラスは、NakuruToolプロジェクトにおけるMVVMパターンの実装を効率化し、保守性を向上させる基盤クラスです。Livetフレームワークとの完全な互換性を保ちながら、開発効率とパフォーマンスの両立を実現します。

適切な使用により、以下の利点が得られます：
- コードの簡潔性と可読性の向上
- プロパティ変更通知の実装ミス削減
- パフォーマンスの最適化
- 保守性の向上
- 基底クラスとの安全な統合（名前競合回避）