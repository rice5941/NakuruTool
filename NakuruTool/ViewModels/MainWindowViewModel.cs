using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using NakuruTool.Models;
using NakuruTool.Views;
using NakuruTool.Services;

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
                if (_selectedLanguage == value)
                {
                    return;
                }
                _selectedLanguage = value;
                RaisePropertyChanged();
                
                // 言語を変更
                if (!string.IsNullOrEmpty(value) && LanguageManager.CurrentLanguage != value)
                {
                    LanguageManager.ChangeLanguage(value);
                    
                    // 利用可能な言語リストを更新（表示名が変わるため）
                    UpdateLanguageDisplayNames();
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
                if (_availableLanguages == value)
                {
                    return;
                }
                _availableLanguages = value;
                RaisePropertyChanged();
            }
        }

        #region 関数
        
        /// <summary>
        /// ViewModelを初期化します
        /// </summary>
        public void Initialize()
        {
            InitializeLanguages();
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

        /// <summary>
        /// 設定ダイアログを表示します
        /// </summary>
        private void ShowSettings()
        {
            var message = new TransitionMessage("ShowSettings");
            Messenger.Raise(message);
        }
        
        #endregion
    }
}
