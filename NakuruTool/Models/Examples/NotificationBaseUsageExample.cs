/*
using System;
using System.Collections.ObjectModel;
using NakuruTool.Models;

namespace NakuruTool.Models.Examples
{
    /// <summary>
    /// NotificationBaseクラスの使用例を示すサンプルクラス
    /// 様々なSetPropertyメソッドの活用方法を実演します
    /// 
    /// 注意: このクラスは参考用のサンプルコードです。
    /// 変更前後処理（onChanging, onChanged）が必要な場合のパターンを示しています。
    /// Livet.RaisePropertyChangedIfSetでは変更前後処理がサポートされていないため、
    /// そのような処理が必要な場合は独自のSetPropertyメソッドを検討してください。
    /// </summary>
    public class NotificationBaseUsageExample : NotificationBase
    {
        #region コンストラクタ

        /// <summary>
        /// NotificationBaseUsageExampleクラスの新しいインスタンスを初期化します
        /// </summary>
        public NotificationBaseUsageExample()
        {
            _name = string.Empty;
            _age = 0;
            _isActive = false;
            _items = new ObservableCollection<string>();
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// 名前プロパティ
        /// 基本的なSetPropertyの使用例
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { SetProperty(ref _name, value); }
        }

        /// <summary>
        /// 年齢プロパティ
        /// 値の範囲チェックを含むSetPropertyの使用例
        /// </summary>
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
                            throw new ArgumentOutOfRangeException(nameof(value), "年齢は0から150の間で設定してください。");
                        }
                    },
                    // 変更後処理：関連プロパティの更新通知
                    (oldValue, newValue) => 
                    {
                        RaisePropertyChanged(nameof(IsAdult));
                        OnAgeChanged?.Invoke(oldValue, newValue);
                    }
                );
            }
        }

        /// <summary>
        /// アクティブ状態プロパティ
        /// 関連プロパティと同時に更新通知を行う例
        /// </summary>
        public bool IsActive
        {
            get { return _isActive; }
            set 
            { 
                SetProperty(ref _isActive, value, 
                    new string[] { nameof(StatusText), nameof(StatusColor) }); 
            }
        }

        /// <summary>
        /// 成人かどうかを示すプロパティ（読み取り専用）
        /// Ageプロパティに依存する計算プロパティ
        /// </summary>
        public bool IsAdult
        {
            get { return _age >= 20; }
        }

        /// <summary>
        /// 状態テキストプロパティ（読み取り専用）
        /// IsActiveプロパティに依存する計算プロパティ
        /// </summary>
        public string StatusText
        {
            get { return _isActive ? "アクティブ" : "非アクティブ"; }
        }

        /// <summary>
        /// 状態色プロパティ（読み取り専用）
        /// IsActiveプロパティに依存する計算プロパティ
        /// </summary>
        public string StatusColor
        {
            get { return _isActive ? "Green" : "Gray"; }
        }

        /// <summary>
        /// アイテムコレクションプロパティ
        /// コレクション型プロパティの実装例
        /// </summary>
        public ObservableCollection<string> Items
        {
            get { return _items; }
            set { SetProperty(ref _items, value, new string[] { nameof(ItemCount) }); }
        }

        /// <summary>
        /// アイテム数プロパティ（読み取り専用）
        /// Itemsプロパティに依存する計算プロパティ
        /// </summary>
        public int ItemCount
        {
            get { return _items?.Count ?? 0; }
        }

        #endregion

        #region イベント

        /// <summary>
        /// 年齢が変更された時に発生するイベント
        /// </summary>
        public event Action<int, int> OnAgeChanged;

        #endregion

        #region 関数

        /// <summary>
        /// アイテムを追加します
        /// </summary>
        /// <param name="item">追加するアイテム</param>
        public void AddItem(string item)
        {
            if (string.IsNullOrEmpty(item) == false)
            {
                _items.Add(item);
                // コレクションの内容が変更されたので、依存プロパティの更新通知を送信
                RaisePropertyChanged(nameof(ItemCount));
            }
        }

        /// <summary>
        /// アイテムを削除します
        /// </summary>
        /// <param name="item">削除するアイテム</param>
        /// <returns>削除に成功した場合はtrue</returns>
        public bool RemoveItem(string item)
        {
            bool result = _items.Remove(item);
            if (result)
            {
                // コレクションの内容が変更されたので、依存プロパティの更新通知を送信
                RaisePropertyChanged(nameof(ItemCount));
            }
            return result;
        }

        /// <summary>
        /// すべてのアイテムを削除します
        /// </summary>
        public void ClearItems()
        {
            _items.Clear();
            // コレクションの内容が変更されたので、依存プロパティの更新通知を送信
            RaisePropertyChanged(nameof(ItemCount));
        }

        /// <summary>
        /// データをリセットします
        /// </summary>
        public void Reset()
        {
            // 複数のプロパティを一度にリセット
            SetProperty(ref _name, string.Empty);
            SetProperty(ref _age, 0);
            SetProperty(ref _isActive, false);
            
            _items.Clear();
            RaisePropertyChanged(nameof(ItemCount));
        }

        /// <summary>
        /// このインスタンスの文字列表現を取得します
        /// </summary>
        /// <returns>インスタンスの文字列表現</returns>
        public override string ToString()
        {
            return $"Name: {Name}, Age: {Age}, IsActive: {IsActive}, Items: {ItemCount}";
        }

        #endregion

        #region メンバ変数

        /// <summary>
        /// 名前のバッキングフィールド
        /// </summary>
        private string _name;

        /// <summary>
        /// 年齢のバッキングフィールド
        /// </summary>
        private int _age;

        /// <summary>
        /// アクティブ状態のバッキングフィールド
        /// </summary>
        private bool _isActive;

        /// <summary>
        /// アイテムコレクションのバッキングフィールド
        /// </summary>
        private ObservableCollection<string> _items;

        #endregion
    }
}
*/