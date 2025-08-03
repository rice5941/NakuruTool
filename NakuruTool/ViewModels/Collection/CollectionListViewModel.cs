using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using Livet;
using NakuruTool.Services;

namespace NakuruTool.ViewModels.Collection
{
    /// <summary>
    /// コレクション一覧表示のViewModel
    /// </summary>
    public class CollectionListViewModel : ViewModel
    {
        #region コンストラクタ
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CollectionListViewModel()
        {
            _collections = new ObservableCollection<CollectionViewModel>();
            _filteredCollections = new ObservableCollection<CollectionViewModel>();
            _collectionFilterText = string.Empty;
        }
        
        #endregion

        #region プロパティ

        /// <summary>
        /// コレクション一覧（全体）
        /// </summary>
        public ObservableCollection<CollectionViewModel> Collections
        {
            get { return _collections; }
            set
            {
                if (RaisePropertyChangedIfSet(ref _collections, value, nameof(OwnedCollectionsHeader)))
                {
                    ApplyCollectionFilter();
                }
            }
        }

        /// <summary>
        /// フィルタリングされたコレクション一覧
        /// </summary>
        public ObservableCollection<CollectionViewModel> FilteredCollections
        {
            get { return _filteredCollections; }
            set
            {
                RaisePropertyChangedIfSet(ref _filteredCollections, value);
            }
        }

        /// <summary>
        /// コレクション検索テキスト
        /// </summary>
        public string CollectionFilterText
        {
            get { return _collectionFilterText; }
            set
            {
                if (RaisePropertyChangedIfSet(ref _collectionFilterText, value))
                {
                    ApplyCollectionFilter();
                }
            }
        }

        /// <summary>
        /// 所持コレクションのヘッダー（件数付き）
        /// </summary>
        public string OwnedCollectionsHeader
        {
            get
            {
                var count = Collections?.Count ?? 0;
                if (count > 0)
                {
                    var template = LanguageManager.GetString("OwnedCollectionsWithCount");
                    if (string.IsNullOrEmpty(template) || template == "OwnedCollectionsWithCount")
                    {
                        template = Application.Current?.Resources["OwnedCollectionsWithCount"] as string ?? "所持コレクション ({0})";
                    }
                    return string.Format(template, count);
                }
                else
                {
                    var template = LanguageManager.GetString("OwnedCollections");
                    if (string.IsNullOrEmpty(template) || template == "OwnedCollections")
                    {
                        template = Application.Current?.Resources["OwnedCollections"] as string ?? "所持コレクション";
                    }
                    return template;
                }
            }
        }

        /// <summary>
        /// 選択されたコレクション
        /// </summary>
        public CollectionViewModel SelectedCollection
        {
            get { return _selectedCollection; }
            set
            {
                if (RaisePropertyChangedIfSet(ref _selectedCollection, value))
                {
                    OnCollectionSelected();
                }
            }
        }

        #endregion

        #region イベント

        /// <summary>
        /// コレクション選択変更イベント
        /// </summary>
        public event EventHandler<CollectionSelectedEventArgs> CollectionSelected;

        #endregion

        #region 関数

        /// <summary>
        /// コレクション一覧を設定します
        /// </summary>
        /// <param name="collections">コレクション一覧</param>
        public void SetCollections(ObservableCollection<Models.Collection.Collection> collections)
        {
            // 新しいObservableCollectionを作成（参照が変わることで適切にイベントが発火）
            var newCollections = new ObservableCollection<CollectionViewModel>();
            foreach (var collection in collections)
            {
                var collectionViewModel = CollectionViewModel.FromCollection(collection);
                newCollections.Add(collectionViewModel);
            }
            Collections = newCollections;
            
            // フィルタを適用
            ApplyCollectionFilter();
            
            // 最初のコレクションがあれば自動選択
            if (FilteredCollections.Count > 0)
            {
                SelectedCollection = FilteredCollections[0];
            }
        }

        /// <summary>
        /// コレクションフィルタを適用します
        /// </summary>
        private void ApplyCollectionFilter()
        {
            try
            {
                if (Collections == null)
                {
                    FilteredCollections?.Clear();
                    return;
                }

                if (FilteredCollections == null)
                {
                    FilteredCollections = new ObservableCollection<CollectionViewModel>();
                }

                FilteredCollections.Clear();

                if (string.IsNullOrWhiteSpace(CollectionFilterText))
                {
                    // フィルタが空の場合は全て表示
                    foreach (var collection in Collections)
                    {
                        FilteredCollections.Add(collection);
                    }
                }
                else
                {
                    // フィルタテキストに一致するコレクションのみ表示（ToLowerInvariant使用）
                    var filterLower = CollectionFilterText.ToLowerInvariant();
                    foreach (var collection in Collections)
                    {
                        if (collection.Name.ToLowerInvariant().Contains(filterLower))
                        {
                            FilteredCollections.Add(collection);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply collection filter: {ex.Message}");
            }
        }

        /// <summary>
        /// コレクション選択時の処理
        /// </summary>
        private void OnCollectionSelected()
        {
            // イベントを発火してタブViewModel等に通知
            CollectionSelected?.Invoke(this, new CollectionSelectedEventArgs(SelectedCollection));
        }

        #endregion

        #region メンバ変数

        /// <summary>
        /// コレクション一覧
        /// </summary>
        private ObservableCollection<CollectionViewModel> _collections;

        /// <summary>
        /// フィルタリングされたコレクション一覧
        /// </summary>
        private ObservableCollection<CollectionViewModel> _filteredCollections;

        /// <summary>
        /// コレクション検索テキスト
        /// </summary>
        private string _collectionFilterText;
        
        /// <summary>
        /// 選択されたコレクション
        /// </summary>
        private CollectionViewModel _selectedCollection;

        #endregion
    }

    #region イベント引数クラス

    /// <summary>
    /// コレクション選択変更イベント引数
    /// </summary>
    public class CollectionSelectedEventArgs : EventArgs
    {
        /// <summary>
        /// 選択されたコレクション
        /// </summary>
        public CollectionViewModel SelectedCollection { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="selectedCollection">選択されたコレクション</param>
        public CollectionSelectedEventArgs(CollectionViewModel selectedCollection)
        {
            SelectedCollection = selectedCollection;
        }
    }

    #endregion
}