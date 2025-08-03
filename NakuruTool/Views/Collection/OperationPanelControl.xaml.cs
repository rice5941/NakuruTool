using System;
using System.Windows;
using System.Windows.Controls;
using NakuruTool.ViewModels.Collection;

namespace NakuruTool.Views.Collection
{
    /// <summary>
    /// 操作パネルのUserControl
    /// </summary>
    public partial class OperationPanelControl : UserControl
    {
        #region 依存関係プロパティ

        /// <summary>
        /// osu!フォルダパスの依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty OsuFolderPathProperty =
            DependencyProperty.Register(
                nameof(OsuFolderPath),
                typeof(string),
                typeof(OperationPanelControl),
                new PropertyMetadata(string.Empty, OnOsuFolderPathChanged));

        #endregion

        #region プロパティ

        /// <summary>
        /// osu!フォルダパス
        /// </summary>
        public string OsuFolderPath
        {
            get { return (string)GetValue(OsuFolderPathProperty); }
            set { SetValue(OsuFolderPathProperty, value); }
        }

        /// <summary>
        /// 操作パネルViewModel
        /// </summary>
        public OperationPanelViewModel ViewModel { get; set; }


        #endregion

        #region イベント

        /// <summary>
        /// コレクション読み込み完了イベント
        /// </summary>
        public event EventHandler<CollectionsLoadedEventArgs> CollectionsLoaded;

        /// <summary>
        /// コレクション読み込みエラーイベント
        /// </summary>
        public event EventHandler<CollectionLoadErrorEventArgs> CollectionLoadError;

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OperationPanelControl()
        {
            InitializeComponent();
            
            // DataContextChangedイベントでViewModelを設定
            DataContextChanged += OnDataContextChanged;
            
            // Unloadedイベントでクリーンアップ
            Unloaded += OnControlUnloaded;
        }

        /// <summary>
        /// DataContextChanged時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            SetupViewModel();
        }

        #endregion

        #region 関数

        /// <summary>
        /// ViewModelを設定します（DataContextChangedで自動的に呼ばれる）
        /// </summary>
        private void SetupViewModel()
        {
            // 既存のViewModelのイベントハンドラを解除
            if (ViewModel != null)
            {
                ViewModel.CollectionsLoaded -= OnCollectionsLoaded;
                ViewModel.CollectionLoadError -= OnCollectionLoadError;
            }

            ViewModel = DataContext as OperationPanelViewModel;

            // 新しいViewModelのイベントハンドラを設定
            if (ViewModel != null)
            {
                ViewModel.CollectionsLoaded += OnCollectionsLoaded;
                ViewModel.CollectionLoadError += OnCollectionLoadError;
            }
        }

        /// <summary>
        /// osu!フォルダパス変更時の処理
        /// </summary>
        /// <param name="d">依存関係オブジェクト</param>
        /// <param name="e">イベント引数</param>
        private static void OnOsuFolderPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            // 依存関係プロパティの変更は記録するだけ
            // 実際の更新処理はOperationPanelViewModelがConfigManagerを監視して行う
            System.Diagnostics.Debug.WriteLine($"[OperationPanelControl] OsuFolderPath dependency property changed: '{e.OldValue}' -> '{e.NewValue}'");
        }

        /// <summary>
        /// コレクション読み込み完了時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnCollectionsLoaded(object sender, CollectionsLoadedEventArgs e)
        {
            CollectionsLoaded?.Invoke(this, e);
        }

        /// <summary>
        /// コレクション読み込みエラー時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnCollectionLoadError(object sender, CollectionLoadErrorEventArgs e)
        {
            CollectionLoadError?.Invoke(this, e);
        }

        #endregion

        #region リソースクリーンアップ

        /// <summary>
        /// コントロールアンロード時のクリーンアップ
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnControlUnloaded(object sender, RoutedEventArgs e)
        {
            // DataContextChangedイベントハンドラを解除
            DataContextChanged -= OnDataContextChanged;
            
            if (ViewModel != null)
            {
                ViewModel.CollectionsLoaded -= OnCollectionsLoaded;
                ViewModel.CollectionLoadError -= OnCollectionLoadError;
            }
        }

        #endregion
    }
}