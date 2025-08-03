using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using Livet;
using Livet.Commands;
using NakuruTool.Models.Collection;
using NakuruTool.Models.Export;
using NakuruTool.Services;

namespace NakuruTool.ViewModels.Collection
{
    /// <summary>
    /// 操作パネルのViewModel
    /// </summary>
    public class OperationPanelViewModel : ViewModel
    {
        #region コンストラクタ
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public OperationPanelViewModel()
        {
            _collectionDomain = new CollectionDomain();
            _exportDomain = new ExportDomain();
            _isLoading = false;
            _isExportInProgress = false;
            _statusMessage = GetInitialStatusMessage();
            
            // ConfigManagerのOsuFolderPath変更イベントを監視
            ConfigManager.OsuFolderPathChanged += OnConfigManagerOsuFolderPathChanged;
            
            // 初期値を設定
            var initialPath = ConfigManager.CurrentConfig.OsuFolderPath ?? string.Empty;
            if (string.IsNullOrEmpty(initialPath) == false)
            {
                System.Diagnostics.Debug.WriteLine($"[OperationPanelViewModel] Initial OsuFolderPath: '{initialPath}'");
                OsuFolderPath = initialPath;
            }
        }
        
        #endregion

        #region プロパティ

        /// <summary>
        /// 読み込み状態
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (RaisePropertyChangedIfSet(ref _isLoading, value))
                {
                    LoadCollectionsCommand?.RaiseCanExecuteChanged();
                }
            }
        }

        /// <summary>
        /// ステータスメッセージ
        /// </summary>
        public string StatusMessage
        {
            get { return _statusMessage; }
            set
            {
                RaisePropertyChangedIfSet(ref _statusMessage, value);
            }
        }

        /// <summary>
        /// osu!フォルダパス
        /// </summary>
        public string OsuFolderPath
        {
            get { return _osuFolderPath; }
            set
            {
                if (RaisePropertyChangedIfSet(ref _osuFolderPath, value))
                {
                    LoadCollectionsCommand?.RaiseCanExecuteChanged();
                    UpdateStatusMessage();
                }
            }
        }

        /// <summary>
        /// コレクション読み込みコマンド
        /// </summary>
        public ViewModelCommand LoadCollectionsCommand
        {
            get
            {
                if (_loadCollectionsCommand == null)
                {
                    _loadCollectionsCommand = new ViewModelCommand(LoadCollections, CanLoadCollections);
                }
                return _loadCollectionsCommand;
            }
        }

        /// <summary>
        /// コレクション一覧（MainWindowViewModelとの連携用）
        /// </summary>
        public ObservableCollection<CollectionViewModel> Collections
        {
            get { return _collections; }
            set
            {
                // 値が実際に変更される場合のみ処理
                if (_collections != value)
                {
                    // 既存のコレクションのPropertyChangedイベントを解除
                    UnsubscribeFromCollectionEvents();
                    
                    // 値を更新
                    RaisePropertyChangedIfSet(ref _collections, value);
                    
                    // 新しいコレクションのPropertyChangedイベントを購読
                    SubscribeToCollectionEvents();
                    
                    // 関連プロパティの更新
                    RaisePropertyChanged(nameof(SelectedCollectionCount));
                    RaisePropertyChanged(nameof(SelectedCollectionCountText));
                    
                    // コマンドの状態更新
                    UpdateCommandStates();
                }
            }
        }

        /// <summary>
        /// エクスポート処理中かどうか
        /// </summary>
        public bool IsExportInProgress
        {
            get { return _isExportInProgress; }
            set
            {
                if (RaisePropertyChangedIfSet(ref _isExportInProgress, value))
                {
                    UpdateCommandStates();
                }
            }
        }

        /// <summary>
        /// 選択されたコレクション数
        /// </summary>
        public int SelectedCollectionCount
        {
            get
            {
                return Collections?.Count(x => x.IsSelected) ?? 0;
            }
        }

        /// <summary>
        /// 選択されたコレクション数の表示テキスト
        /// </summary>
        public string SelectedCollectionCountText
        {
            get
            {
                var count = SelectedCollectionCount;
                if (count > 0)
                {
                    var template = LanguageManager.GetString("SelectedCollectionsCount");
                    if (string.IsNullOrEmpty(template) || template == "SelectedCollectionsCount")
                    {
                        template = "選択中: {0}個";
                    }
                    return string.Format(template, count);
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// 選択されたコレクションのエクスポートコマンド
        /// </summary>
        public ViewModelCommand ExportSelectedCollectionsCommand
        {
            get
            {
                return _exportSelectedCollectionsCommand ??
                    (_exportSelectedCollectionsCommand = new ViewModelCommand(ExportSelectedCollections, CanExportSelectedCollections));
            }
        }

        /// <summary>
        /// 全コレクション選択コマンド
        /// </summary>
        public ViewModelCommand SelectAllCollectionsCommand
        {
            get
            {
                return _selectAllCollectionsCommand ??
                    (_selectAllCollectionsCommand = new ViewModelCommand(SelectAllCollections, CanSelectAllCollections));
            }
        }

        /// <summary>
        /// 全コレクション選択解除コマンド
        /// </summary>
        public ViewModelCommand UnselectAllCollectionsCommand
        {
            get
            {
                return _unselectAllCollectionsCommand ??
                    (_unselectAllCollectionsCommand = new ViewModelCommand(UnselectAllCollections, CanUnselectAllCollections));
            }
        }

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

        #region 関数

        /// <summary>
        /// osu!フォルダパスを更新します
        /// </summary>
        /// <param name="folderPath">osu!フォルダパス</param>
        public void UpdateOsuFolderPath(string folderPath)
        {
            var newPath = folderPath ?? string.Empty;
            if (OsuFolderPath != newPath)
            {
                System.Diagnostics.Debug.WriteLine($"[OperationPanelViewModel] OsuFolderPath updated: '{newPath}'");
            }
            OsuFolderPath = newPath;
        }

        /// <summary>
        /// ConfigManagerのOsuFolderPath変更時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="newPath">新しいosu!フォルダパス</param>
        private void OnConfigManagerOsuFolderPathChanged(object sender, string newPath)
        {
            System.Diagnostics.Debug.WriteLine($"[OperationPanelViewModel] ConfigManager OsuFolderPath changed: '{newPath}'");
            
            // UIスレッドで実行
            Application.Current.Dispatcher.Invoke(() =>
            {
                OsuFolderPath = newPath ?? string.Empty;
            });
        }

        /// <summary>
        /// 初期ステータスメッセージを取得します
        /// </summary>
        /// <returns>初期ステータスメッセージ</returns>
        private string GetInitialStatusMessage()
        {
            var initMessage = LanguageManager.GetString("PleaseSelectOsuFolderFromSettings");
            if (string.IsNullOrEmpty(initMessage) || initMessage == "PleaseSelectOsuFolderFromSettings")
            {
                initMessage = "設定からosu!フォルダを選択してください";
            }
            return initMessage;
        }

        /// <summary>
        /// ステータスメッセージを更新します
        /// </summary>
        private void UpdateStatusMessage()
        {
            if (string.IsNullOrEmpty(OsuFolderPath))
            {
                var settingsMessage = LanguageManager.GetString("PleaseSelectOsuFolderFromSettings");
                if (string.IsNullOrEmpty(settingsMessage) || settingsMessage == "PleaseSelectOsuFolderFromSettings")
                {
                    settingsMessage = "設定からosu!フォルダを選択してください";
                }
                StatusMessage = settingsMessage;
            }
            else if (string.IsNullOrEmpty(StatusMessage) || StatusMessage.Contains("設定から") || StatusMessage.Contains("settings"))
            {
                StatusMessage = string.Empty;
            }
        }

        /// <summary>
        /// コレクション読み込み可能かどうか
        /// </summary>
        /// <returns>読み込み可能な場合はtrue</returns>
        private bool CanLoadCollections()
        {
            return (IsLoading == false) && (string.IsNullOrEmpty(OsuFolderPath) == false);
        }

        /// <summary>
        /// コレクションを読み込みます
        /// </summary>
        private async void LoadCollections()
        {
            try
            {
                IsLoading = true;
                StatusMessage = LanguageManager.GetString("LoadingCollections") ?? "コレクションを読み込み中...";

                var collections = await _collectionDomain.LoadCollectionsAsync(OsuFolderPath);
                var observableCollections = new ObservableCollection<Models.Collection.Collection>(collections);

                // コレクション読み込み完了イベントを発火
                CollectionsLoaded?.Invoke(this, new CollectionsLoadedEventArgs(observableCollections));

                // ステータスメッセージをクリア
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                var errorPrefix = LanguageManager.GetString("Error");
                if (string.IsNullOrEmpty(errorPrefix) || errorPrefix == "Error")
                {
                    errorPrefix = Application.Current?.Resources["Error"] as string ?? "エラー";
                }
                StatusMessage = $"{errorPrefix}: {ex.Message}";

                // エラーイベントを発火
                CollectionLoadError?.Invoke(this, new CollectionLoadErrorEventArgs(ex));
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region エクスポート関連関数

        /// <summary>
        /// コレクションのプロパティ変更時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnCollectionPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(CollectionViewModel.IsSelected))
            {
                RaisePropertyChanged(nameof(SelectedCollectionCount));
                RaisePropertyChanged(nameof(SelectedCollectionCountText));
                UpdateCommandStates();
            }
        }

        /// <summary>
        /// コレクションイベントの購読
        /// </summary>
        private void SubscribeToCollectionEvents()
        {
            if (Collections != null)
            {
                foreach (var collection in Collections)
                {
                    collection.PropertyChanged += OnCollectionPropertyChanged;
                }
            }
        }

        /// <summary>
        /// コレクションイベントの購読解除
        /// </summary>
        private void UnsubscribeFromCollectionEvents()
        {
            if (Collections != null)
            {
                foreach (var collection in Collections)
                {
                    collection.PropertyChanged -= OnCollectionPropertyChanged;
                }
            }
        }

        /// <summary>
        /// コマンドの状態を更新します
        /// </summary>
        private void UpdateCommandStates()
        {
            ExportSelectedCollectionsCommand?.RaiseCanExecuteChanged();
            SelectAllCollectionsCommand?.RaiseCanExecuteChanged();
            UnselectAllCollectionsCommand?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// 選択されたコレクションをエクスポート可能かどうか
        /// </summary>
        /// <returns>エクスポート可能な場合はtrue</returns>
        private bool CanExportSelectedCollections()
        {
            if (IsExportInProgress)
            {
                return false;
            }

            if (SelectedCollectionCount == 0)
            {
                return false;
            }

            if (string.IsNullOrWhiteSpace(OsuFolderPath))
            {
                return false;
            }

            var selectedCollections = GetSelectedCollections();
            return _exportDomain.CanExport(selectedCollections, OsuFolderPath);
        }

        /// <summary>
        /// 選択されたコレクションをエクスポートします
        /// </summary>
        private async void ExportSelectedCollections()
        {
            try
            {
                IsExportInProgress = true;
                
                var selectedCollections = GetSelectedCollections();

                if (selectedCollections.Count == 0 || string.IsNullOrWhiteSpace(OsuFolderPath))
                {
                    return;
                }

                // 進行状況をステータスメッセージで表示
                var progress = new Progress<string>(message =>
                {
                    StatusMessage = message;
                });

                var result = await _exportDomain.ExportCollectionsAsync(selectedCollections, OsuFolderPath, progress);

                // 完了メッセージを表示
                var summaryMessage = _exportDomain.GenerateSummaryMessage(result);
                var title = LanguageManager.GetString("ExportCompleted") ?? "エクスポート完了";
                
                MessageBox.Show(summaryMessage, title, MessageBoxButton.OK, 
                    result.IsSuccess ? MessageBoxImage.Information : MessageBoxImage.Warning);

                // ステータスメッセージをクリア
                StatusMessage = string.Empty;
            }
            catch (Exception ex)
            {
                var errorTemplate = LanguageManager.GetString("ExportError") ?? "エクスポート中にエラーが発生しました: {0}";
                var errorMessage = string.Format(errorTemplate, ex.Message);
                var errorTitle = LanguageManager.GetString("Error") ?? "エラー";
                
                MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);

                // ステータスメッセージをクリア
                StatusMessage = string.Empty;
            }
            finally
            {
                IsExportInProgress = false;
            }
        }

        /// <summary>
        /// 全コレクション選択可能かどうか
        /// </summary>
        /// <returns>選択可能な場合はtrue</returns>
        private bool CanSelectAllCollections()
        {
            return (IsExportInProgress == false) && (Collections?.Count > 0);
        }

        /// <summary>
        /// 全コレクションを選択します
        /// </summary>
        private void SelectAllCollections()
        {
            if (Collections == null)
            {
                return;
            }

            // パフォーマンス最適化: イベント監視を一時停止
            UnsubscribeFromCollectionEvents();
            
            try
            {
                // 全コレクションを選択（PropertyChangedは発火するが監視されない）
                foreach (var collection in Collections)
                {
                    collection.IsSelected = true;
                }
            }
            finally
            {
                // イベント監視を再開
                SubscribeToCollectionEvents();
                
                // 一括で更新通知（O(1)の処理）
                RaisePropertyChanged(nameof(SelectedCollectionCount));
                RaisePropertyChanged(nameof(SelectedCollectionCountText));
                UpdateCommandStates();
            }
        }

        /// <summary>
        /// 全コレクション選択解除可能かどうか
        /// </summary>
        /// <returns>選択解除可能な場合はtrue</returns>
        private bool CanUnselectAllCollections()
        {
            return (IsExportInProgress == false) && (SelectedCollectionCount > 0);
        }

        /// <summary>
        /// 全コレクションの選択を解除します
        /// </summary>
        private void UnselectAllCollections()
        {
            if (Collections == null)
            {
                return;
            }

            // パフォーマンス最適化: イベント監視を一時停止
            UnsubscribeFromCollectionEvents();
            
            try
            {
                // 全コレクション選択を解除（PropertyChangedは発火するが監視されない）
                foreach (var collection in Collections)
                {
                    collection.IsSelected = false;
                }
            }
            finally
            {
                // イベント監視を再開
                SubscribeToCollectionEvents();
                
                // 一括で更新通知（O(1)の処理）
                RaisePropertyChanged(nameof(SelectedCollectionCount));
                RaisePropertyChanged(nameof(SelectedCollectionCountText));
                UpdateCommandStates();
            }
        }

        /// <summary>
        /// 選択されたコレクションのリストを取得します
        /// </summary>
        /// <returns>選択されたコレクションのリスト</returns>
        private List<Models.Collection.Collection> GetSelectedCollections()
        {
            if (Collections == null)
            {
                return new List<Models.Collection.Collection>();
            }

            return Collections
                .Where(x => x.IsSelected && x.OriginalCollection != null)
                .Select(x => x.OriginalCollection)
                .ToList();
        }

        #endregion

        #endregion

        #region メンバ変数

        /// <summary>
        /// コレクションドメイン
        /// </summary>
        private readonly CollectionDomain _collectionDomain;

        /// <summary>
        /// 読み込み状態
        /// </summary>
        private bool _isLoading;

        /// <summary>
        /// ステータスメッセージ
        /// </summary>
        private string _statusMessage;

        /// <summary>
        /// osu!フォルダパス
        /// </summary>
        private string _osuFolderPath = string.Empty;

        /// <summary>
        /// コレクション読み込みコマンド
        /// </summary>
        private ViewModelCommand _loadCollectionsCommand;

        /// <summary>
        /// エクスポートドメイン
        /// </summary>
        private readonly ExportDomain _exportDomain;

        /// <summary>
        /// コレクション一覧
        /// </summary>
        private ObservableCollection<CollectionViewModel> _collections;

        /// <summary>
        /// エクスポート処理中かどうか
        /// </summary>
        private bool _isExportInProgress;

        /// <summary>
        /// 選択されたコレクションのエクスポートコマンドの実体
        /// </summary>
        private ViewModelCommand _exportSelectedCollectionsCommand;

        /// <summary>
        /// 全コレクション選択コマンドの実体
        /// </summary>
        private ViewModelCommand _selectAllCollectionsCommand;

        /// <summary>
        /// 全コレクション選択解除コマンドの実体
        /// </summary>
        private ViewModelCommand _unselectAllCollectionsCommand;

        #endregion

        #region クリーンアップ

        /// <summary>
        /// ViewModelのクリーンアップ
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // ConfigManagerのイベントハンドラを解除
                ConfigManager.OsuFolderPathChanged -= OnConfigManagerOsuFolderPathChanged;
                
                // コレクションのイベントハンドラを解除
                UnsubscribeFromCollectionEvents();
            }
            base.Dispose(disposing);
        }

        #endregion
    }

    #region イベント引数クラス

    /// <summary>
    /// コレクション読み込み完了イベント引数
    /// </summary>
    public class CollectionsLoadedEventArgs : EventArgs
    {
        /// <summary>
        /// 読み込まれたコレクション一覧
        /// </summary>
        public ObservableCollection<Models.Collection.Collection> Collections { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="collections">読み込まれたコレクション一覧</param>
        public CollectionsLoadedEventArgs(ObservableCollection<Models.Collection.Collection> collections)
        {
            Collections = collections ?? new ObservableCollection<Models.Collection.Collection>();
        }
    }

    /// <summary>
    /// コレクション読み込みエラーイベント引数
    /// </summary>
    public class CollectionLoadErrorEventArgs : EventArgs
    {
        /// <summary>
        /// エラー例外
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="exception">エラー例外</param>
        public CollectionLoadErrorEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }

    #endregion
}