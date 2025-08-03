using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Livet;
using Livet.Commands;
using NakuruTool.Models.Collection;
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
            _isLoading = false;
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