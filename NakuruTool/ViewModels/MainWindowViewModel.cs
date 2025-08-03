using System;
using System.Collections.ObjectModel;
using System.Windows;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using NakuruTool.Services;
using NakuruTool.ViewModels.Tabs;
using NakuruTool.ViewModels.Events;

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
    /// メインウィンドウのViewModel（タブベース設計）
    /// </summary>
    public class MainWindowViewModel : ViewModel
    {
        #region コンストラクタ
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public MainWindowViewModel()
        {
            // 初期状態を設定
            _statusMessage = "";
            _isLoading = false;
            
            // コレクションタブViewModelを初期化
            InitializeCollectionTabViewModel();
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
        /// コレクションタブのViewModel
        /// </summary>
        public CollectionTabViewModel CollectionTabViewModel
        {
            get { return _collectionTabViewModel; }
            private set
            {
                RaisePropertyChangedIfSet(ref _collectionTabViewModel, value);
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

        /// <summary>
        /// ローディング状態
        /// </summary>
        public bool IsLoading
        {
            get { return _isLoading; }
            set { RaisePropertyChangedIfSet(ref _isLoading, value); }
        }

        /// <summary>
        /// ステータスメッセージ
        /// </summary>
        public string StatusMessage
        {
            get { return _statusMessage; }
            set { RaisePropertyChangedIfSet(ref _statusMessage, value); }
        }

        #endregion

        #region 関数

        /// <summary>
        /// ViewModelを初期化します
        /// </summary>
        public void Initialize()
        {
            InitializeLanguages();
        }

        /// <summary>
        /// コレクションタブViewModelを初期化します
        /// </summary>
        private void InitializeCollectionTabViewModel()
        {
            CollectionTabViewModel = new CollectionTabViewModel();
            
            // ステータス変更イベントを購読
            CollectionTabViewModel.StatusChanged += OnStatusChanged;
        }
        
        /// <summary>
        /// ステータス変更イベントハンドラ
        /// </summary>
        /// <param name="sender">送信者</param>
        /// <param name="e">イベント引数</param>
        private void OnStatusChanged(object sender, StatusChangedEventArgs e)
        {
            // 複数プロパティをまとめて処理したいためEventArgsを使用する.
            IsLoading = e.IsLoading;
            StatusMessage = e.StatusMessage;
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
        }

        /// <summary>
        /// 設定ダイアログを表示します
        /// </summary>
        private void ShowSettings()
        {
            var message = new TransitionMessage("ShowSettings");
            Messenger.Raise(message);
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
                if (CollectionTabViewModel != null)
                {
                    CollectionTabViewModel.StatusChanged -= OnStatusChanged;
                    CollectionTabViewModel.Dispose();
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
        /// コレクションタブのViewModel
        /// </summary>
        private CollectionTabViewModel _collectionTabViewModel;
        
        /// <summary>
        /// ローディング状態
        /// </summary>
        private bool _isLoading;
        
        /// <summary>
        /// ステータスメッセージ
        /// </summary>
        private string _statusMessage;

        #endregion
    }
}