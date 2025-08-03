using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using NakuruTool.Models;
using NakuruTool.Models.Collection;
using NakuruTool.Views;
using NakuruTool.Services;
using NakuruTool.ViewModels.Collection;

namespace NakuruTool.ViewModels
{
    /// <summary>
    /// 言語選択オプションを表現するクラス
    /// </summary>
    public class LanguageOption
    {
        /// <summary>
        /// 言語コード
        /// </summary>
        public string Code { get; set; }
        
        /// <summary>
        /// 表示名
        /// </summary>
        public string DisplayName { get; set; }
    }

    /// <summary>
    /// メインウィンドウのViewModel
    /// </summary>
    public class MainWindowViewModel : ViewModel
    {

        #region コンストラクタ
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            _collections = new ObservableCollection<CollectionViewModel>();
            _filteredCollections = new ObservableCollection<CollectionViewModel>();
            _beatmapDetails = new BeatmapDetailsViewModel();
            _collectionFilterText = string.Empty;

            // 操作パネルViewModelを初期化
            InitializeOperationPanelViewModel();
        }
        
        #endregion

        #region プロパティ
        
        /// <summary>
        /// 選択された言語コード
        /// </summary>
        public string SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                if (RaisePropertyChangedIfSet(ref _selectedLanguage, value))
                {
                    // 言語を変更
                    if ((string.IsNullOrEmpty(value) == false) && (LanguageManager.CurrentLanguage != value))
                    {
                        LanguageManager.ChangeLanguage(value);
                        
                        // 利用可能な言語リストを更新（表示名が変わるため）
                        UpdateLanguageDisplayNames();
                    }
                }
            }
        }

        /// <summary>
        /// 利用可能な言語一覧
        /// </summary>
        public ObservableCollection<LanguageOption> AvailableLanguages
        {
            get { return _availableLanguages; }
            set
            {
                RaisePropertyChangedIfSet(ref _availableLanguages, value);
            }
        }

        /// <summary>
        /// コレクション一覧（全体）
        /// </summary>
        public ObservableCollection<CollectionViewModel> Collections
        {
            get { return _collections; }
            set
            {
                RaisePropertyChangedIfSet(ref _collections, value, nameof(OwnedCollectionsHeader));
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

        /// <summary>
        /// ビートマップ詳細表示のViewModel
        /// </summary>
        public BeatmapDetailsViewModel BeatmapDetails
        {
            get { return _beatmapDetails; }
            set
            {
                RaisePropertyChangedIfSet(ref _beatmapDetails, value);
            }
        }

        /// <summary>
        /// 操作パネルのViewModel
        /// </summary>
        public OperationPanelViewModel OperationPanelViewModel
        {
            get { return _operationPanelViewModel; }
            set
            {
                RaisePropertyChangedIfSet(ref _operationPanelViewModel, value);
            }
        }



        /// <summary>
        /// 設定表示コマンド
        /// </summary>
        public ViewModelCommand ShowSettingsCommand
        {
            get
            {
                return _showSettingsCommand ??
                    (_showSettingsCommand = new ViewModelCommand(ShowSettings));
            }
        }




        
        #endregion

        #region 関数
        
        /// <summary>
        /// ViewModelを初期化します
        /// </summary>
        public void Initialize()
        {
            InitializeLanguages();
            
            // 初期フィルタを適用
            ApplyCollectionFilter();
        }

        /// <summary>
        /// 操作パネルViewModelを初期化します
        /// </summary>
        private void InitializeOperationPanelViewModel()
        {
            _operationPanelViewModel = new OperationPanelViewModel();
            
            // 操作パネルからのイベントを処理
            _operationPanelViewModel.CollectionsLoaded += OnOperationPanelCollectionsLoaded;
            _operationPanelViewModel.CollectionLoadError += OnOperationPanelCollectionLoadError;
        }

        /// <summary>
        /// 操作パネルからのコレクション読み込み完了時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnOperationPanelCollectionsLoaded(object sender, CollectionsLoadedEventArgs e)
        {
            OnCollectionsLoaded(e.Collections);
        }

        /// <summary>
        /// 操作パネルからのコレクション読み込みエラー時の処理
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnOperationPanelCollectionLoadError(object sender, CollectionLoadErrorEventArgs e)
        {
            OnCollectionLoadError(e.Exception);
        }




        /// <summary>
        /// 言語設定を初期化します
        /// </summary>
        private void InitializeLanguages()
        {
            _selectedLanguage = LanguageManager.CurrentLanguage;
            
            AvailableLanguages = new ObservableCollection<LanguageOption>();
            foreach (var lang in LanguageManager.GetAvailableLanguages())
            {
                AvailableLanguages.Add(new LanguageOption
                {
                    Code = lang.Key,
                    DisplayName = LanguageManager.GetLanguageDisplayName(lang.Key)
                });
            }
            
            RaisePropertyChanged(nameof(SelectedLanguage));
        }

        /// <summary>
        /// 言語表示名を更新します
        /// </summary>
        private void UpdateLanguageDisplayNames()
        {
            foreach (var lang in AvailableLanguages)
            {
                lang.DisplayName = LanguageManager.GetLanguageDisplayName(lang.Code);
            }
            RaisePropertyChanged(nameof(AvailableLanguages));
            RaisePropertyChanged(nameof(OwnedCollectionsHeader));
        }

        /// <summary>
        /// 設定ダイアログを表示します
        /// </summary>
        private void ShowSettings()
        {
            var message = new TransitionMessage("ShowSettings");
            Messenger.Raise(message);
        }

        /// <summary>
        /// 操作パネルからのコレクション読み込み完了を処理します
        /// </summary>
        /// <param name="collections">読み込まれたコレクション一覧</param>
        public void OnCollectionsLoaded(ObservableCollection<Models.Collection.Collection> collections)
        {
            Collections.Clear();
            foreach (var collection in collections)
            {
                Collections.Add(CollectionViewModel.FromCollection(collection));
            }
            
            // フィルタを適用
            ApplyCollectionFilter();
            
            // 最初のコレクションがあれば自動選択
            if (FilteredCollections.Count > 0)
            {
                SelectedCollection = FilteredCollections[0];
            }
            
            // ヘッダーの更新を明示的に呼び出し
            RaisePropertyChanged(nameof(OwnedCollectionsHeader));
        }

        /// <summary>
        /// 操作パネルからのコレクション読み込みエラーを処理します
        /// </summary>
        /// <param name="exception">エラー例外</param>
        public void OnCollectionLoadError(Exception exception)
        {
            var errorTemplate = LanguageManager.GetString("ErrorLoadingCollections");
            if (string.IsNullOrEmpty(errorTemplate) || errorTemplate == "ErrorLoadingCollections")
            {
                errorTemplate = Application.Current?.Resources["ErrorLoadingCollections"] as string ?? "コレクションの読み込みに失敗しました。\n\n{0}";
            }
            // \n を実際の改行文字に変換
            errorTemplate = errorTemplate.Replace("\\n", "\n");
            var errorMessage = string.Format(errorTemplate, exception.Message);
            var errorTitle = LanguageManager.GetString("Error");
            if (string.IsNullOrEmpty(errorTitle) || errorTitle == "Error")
            {
                errorTitle = Application.Current?.Resources["Error"] as string ?? "エラー";
            }
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }


        /// <summary>
        /// コレクション選択時の処理
        /// </summary>
        private void OnCollectionSelected()
        {
            if (SelectedCollection != null)
            {
                BeatmapDetails.SetBeatmaps(SelectedCollection.Beatmaps);
            }
            else
            {
                BeatmapDetails.SetBeatmaps(new List<BeatmapViewModel>());
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

        
        #endregion

        #region クリーンアップ

        /// <summary>
        /// ViewModelのクリーンアップ
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // 操作パネルViewModelのイベントハンドラを解除
                if (_operationPanelViewModel != null)
                {
                    _operationPanelViewModel.CollectionsLoaded -= OnOperationPanelCollectionsLoaded;
                    _operationPanelViewModel.CollectionLoadError -= OnOperationPanelCollectionLoadError;
                    _operationPanelViewModel.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #endregion
        
        #region メンバ変数
        
        /// <summary>
        /// 選択された言語コード
        /// </summary>
        private string _selectedLanguage;
        
        /// <summary>
        /// 利用可能な言語一覧
        /// </summary>
        private ObservableCollection<LanguageOption> _availableLanguages;
        
        /// <summary>
        /// 設定表示コマンドの実体
        /// </summary>
        private ViewModelCommand _showSettingsCommand;
        
        
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
        
        /// <summary>
        /// ビートマップ詳細表示のViewModel
        /// </summary>
        private BeatmapDetailsViewModel _beatmapDetails;
        
        
        /// <summary>
        /// 操作パネルのViewModel
        /// </summary>
        private OperationPanelViewModel _operationPanelViewModel;
        
        
        #endregion
    }
}