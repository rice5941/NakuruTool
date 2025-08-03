using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using NakuruTool.ViewModels;
using NakuruTool.ViewModels.Collection;

namespace NakuruTool.Views
{
    /* 
     * If some events were receive from ViewModel, then please use PropertyChangedWeakEventListener and CollectionChangedWeakEventListener.
     * If you want to subscribe custome events, then you can use LivetWeakEventListener.
     * When window closing and any timing, Dispose method of LivetCompositeDisposable is useful to release subscribing events.
     *
     * Those events are managed using WeakEventListener, so it is not occurred memory leak, but you should release explicitly.
     */
    public partial class MainWindow : Window
    {
        #region コンストラクタ

        public MainWindow()
        {
            InitializeComponent();
            InitializeEvents();
        }

        #endregion

        #region 関数

        /// <summary>
        /// イベントを初期化します
        /// </summary>
        private void InitializeEvents()
        {
            // CollectionListControlのイベントをMainWindowViewModelに転送
            CollectionListControl.CollectionsLoaded += OnCollectionListControlCollectionsLoaded;
            CollectionListControl.CollectionLoadError += OnCollectionListControlCollectionLoadError;
        }


        /// <summary>
        /// コレクション読み込み完了時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnCollectionListControlCollectionsLoaded(object sender, CollectionsLoadedEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.OnCollectionsLoaded(e.Collections);
            }
        }

        /// <summary>
        /// コレクション読み込みエラー時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnCollectionListControlCollectionLoadError(object sender, CollectionLoadErrorEventArgs e)
        {
            if (DataContext is MainWindowViewModel viewModel)
            {
                viewModel.OnCollectionLoadError(e.Exception);
            }
        }



        /// <summary>
        /// ウィンドウクローズ時のクリーンアップ
        /// </summary>
        /// <param name="e">イベント引数</param>
        protected override void OnClosed(EventArgs e)
        {
            // イベントハンドラを解除
            if (CollectionListControl != null)
            {
                CollectionListControl.CollectionsLoaded -= OnCollectionListControlCollectionsLoaded;
                CollectionListControl.CollectionLoadError -= OnCollectionListControlCollectionLoadError;
            }
            
            base.OnClosed(e);
        }

        #endregion
    }
}
