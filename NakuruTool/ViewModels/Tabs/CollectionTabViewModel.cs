using System;
using System.Collections.ObjectModel;
using Livet;
using NakuruTool.ViewModels.Collection;
using NakuruTool.ViewModels.Events;

namespace NakuruTool.ViewModels.Tabs
{
    /// <summary>
    /// コレクション管理タブのViewModel
    /// </summary>
    public class CollectionTabViewModel : ViewModel
    {
        #region コンストラクタ
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CollectionTabViewModel()
        {
            InitializeChildViewModels();
            SetupEventHandlers();
        }
        
        #endregion

        #region プロパティ

        /// <summary>
        /// コレクション一覧のViewModel
        /// </summary>
        public CollectionListViewModel CollectionListViewModel
        {
            get { return _collectionListViewModel; }
            private set { RaisePropertyChangedIfSet(ref _collectionListViewModel, value); }
        }

        /// <summary>
        /// コレクションビートマップ一覧のViewModel
        /// </summary>
        public CollectionBeatmapListViewModel CollectionBeatmapListViewModel
        {
            get { return _collectionBeatmapListViewModel; }
            private set { RaisePropertyChangedIfSet(ref _collectionBeatmapListViewModel, value); }
        }

        /// <summary>
        /// 操作パネルのViewModel
        /// </summary>
        public OperationPanelViewModel OperationPanelViewModel
        {
            get { return _operationPanelViewModel; }
            private set { RaisePropertyChangedIfSet(ref _operationPanelViewModel, value); }
        }

        #endregion

        #region イベント
        
        /// <summary>
        /// ステータス変更イベント
        /// </summary>
        public event EventHandler<StatusChangedEventArgs> StatusChanged;
        
        #endregion

        #region 関数

        /// <summary>
        /// 子ViewModelを初期化します
        /// </summary>
        private void InitializeChildViewModels()
        {
            CollectionListViewModel = new CollectionListViewModel();
            CollectionBeatmapListViewModel = new CollectionBeatmapListViewModel();
            OperationPanelViewModel = new OperationPanelViewModel();
        }

        /// <summary>
        /// イベントハンドラを設定します
        /// </summary>
        private void SetupEventHandlers()
        {
            // コレクション選択変更時の処理
            CollectionListViewModel.PropertyChanged += OnCollectionListPropertyChanged;
            
            // 操作パネルからのコレクション読み込み完了時の処理
            OperationPanelViewModel.CollectionsLoaded += OnCollectionsLoaded;
            OperationPanelViewModel.CollectionLoadError += OnCollectionLoadError;
            
            // ステータス変更を上位に中継
            OperationPanelViewModel.PropertyChanged += OnOperationPanelPropertyChanged;
        }

        /// <summary>
        /// CollectionListViewModelのプロパティ変更時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnCollectionListPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CollectionListViewModel.SelectedCollection))
            {
                // 選択されたコレクションのビートマップを表示
                CollectionBeatmapListViewModel.ShowCollectionBeatmaps(CollectionListViewModel.SelectedCollection);
            }
        }

        /// <summary>
        /// コレクション読み込み完了時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnCollectionsLoaded(object sender, CollectionsLoadedEventArgs e)
        {
            // コレクション一覧を更新
            CollectionListViewModel.SetCollections(e.Collections);
            
            // OperationPanelViewModelにもコレクションを設定（エクスポート機能用）
            OperationPanelViewModel.Collections = CollectionListViewModel.Collections;
        }

        /// <summary>
        /// コレクション読み込みエラー時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnCollectionLoadError(object sender, CollectionLoadErrorEventArgs e)
        {
            // エラー処理（必要に応じて上位に伝播）
            System.Diagnostics.Debug.WriteLine($"Collection load error: {e.Exception.Message}");
        }
        
        /// <summary>
        /// OperationPanelViewModelのプロパティ変更時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnOperationPanelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // ステータス関連プロパティの変更を上位に中継
            if (e.PropertyName == nameof(OperationPanelViewModel.IsLoading) ||
                e.PropertyName == nameof(OperationPanelViewModel.StatusMessage))
            {
                StatusChanged?.Invoke(this, new StatusChangedEventArgs(
                    OperationPanelViewModel.IsLoading,
                    OperationPanelViewModel.StatusMessage));
            }
        }

        #endregion

        #region クリーンアップ

        /// <summary>
        /// ViewModelのクリーンアップ
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // イベントハンドラを解除
                if (CollectionListViewModel != null)
                {
                    CollectionListViewModel.PropertyChanged -= OnCollectionListPropertyChanged;
                    CollectionListViewModel.Dispose();
                }

                if (OperationPanelViewModel != null)
                {
                    OperationPanelViewModel.CollectionsLoaded -= OnCollectionsLoaded;
                    OperationPanelViewModel.CollectionLoadError -= OnCollectionLoadError;
                    OperationPanelViewModel.PropertyChanged -= OnOperationPanelPropertyChanged;
                    OperationPanelViewModel.Dispose();
                }

                CollectionBeatmapListViewModel?.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region メンバ変数

        /// <summary>
        /// コレクション一覧のViewModel
        /// </summary>
        private CollectionListViewModel _collectionListViewModel;

        /// <summary>
        /// コレクションビートマップ一覧のViewModel
        /// </summary>
        private CollectionBeatmapListViewModel _collectionBeatmapListViewModel;

        /// <summary>
        /// 操作パネルのViewModel
        /// </summary>
        private OperationPanelViewModel _operationPanelViewModel;

        #endregion
    }
}