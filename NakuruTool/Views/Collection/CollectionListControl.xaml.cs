using System;
using System.Windows;
using System.Windows.Controls;
using NakuruTool.ViewModels;
using NakuruTool.ViewModels.Collection;

namespace NakuruTool.Views.Collection
{
    /// <summary>
    /// CollectionListControl.xaml の相互作用ロジック
    /// </summary>
    public partial class CollectionListControl : UserControl
    {
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
        public CollectionListControl()
        {
            InitializeComponent();
            InitializeEvents();
            
            // Unloadedイベントでクリーンアップ
            Unloaded += OnControlUnloaded;
        }

        #endregion

        #region 関数

        /// <summary>
        /// イベントを初期化します
        /// </summary>
        private void InitializeEvents()
        {
            // OperationPanelのイベントを転送
            OperationPanel.CollectionsLoaded += OnOperationPanelCollectionsLoaded;
            OperationPanel.CollectionLoadError += OnOperationPanelCollectionLoadError;
        }

        /// <summary>
        /// 操作パネルでのコレクション読み込み完了時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnOperationPanelCollectionsLoaded(object sender, CollectionsLoadedEventArgs e)
        {
            CollectionsLoaded?.Invoke(this, e);
        }

        /// <summary>
        /// 操作パネルでのコレクション読み込みエラー時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnOperationPanelCollectionLoadError(object sender, CollectionLoadErrorEventArgs e)
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
            if (OperationPanel != null)
            {
                OperationPanel.CollectionsLoaded -= OnOperationPanelCollectionsLoaded;
                OperationPanel.CollectionLoadError -= OnOperationPanelCollectionLoadError;
            }
        }

        #endregion
    }
}