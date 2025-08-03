using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using Livet;
using Livet.Commands;
using Livet.EventListeners;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.Messaging.Windows;
using NakuruTool.Models;
using NakuruTool.Models.Theme;
using NakuruTool.Services;

namespace NakuruTool.ViewModels
{
    /// <summary>
    /// 設定ダイアログのViewModel
    /// </summary>
    public class SettingsDialogViewModel : ViewModel
    {
        #region メンバ変数
        
        /// <summary>
        /// 現在のテーマタイプ
        /// </summary>
        private ThemeType _currentThemeType;

        /// <summary>
        /// osu!フォルダパス
        /// </summary>
        private string _osuFolderPath;
        
        #endregion

        #region プロパティ
        
        /// <summary>
        /// ライトテーマが選択されているかどうか
        /// </summary>
        public bool IsLightTheme
        {
            get { return _currentThemeType == ThemeType.Light; }
            set
            {
                if (value && _currentThemeType != ThemeType.Light)
                {
                    _currentThemeType = ThemeType.Light;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsDarkTheme));
                    RaisePropertyChanged(nameof(IsOsuTheme));
                    ApplyTheme();
                }
            }
        }

        /// <summary>
        /// ダークテーマが選択されているかどうか
        /// </summary>
        public bool IsDarkTheme
        {
            get { return _currentThemeType == ThemeType.Dark; }
            set
            {
                if (value && _currentThemeType != ThemeType.Dark)
                {
                    _currentThemeType = ThemeType.Dark;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsLightTheme));
                    RaisePropertyChanged(nameof(IsOsuTheme));
                    ApplyTheme();
                }
            }
        }

        /// <summary>
        /// osu!テーマが選択されているかどうか
        /// </summary>
        public bool IsOsuTheme
        {
            get { return _currentThemeType == ThemeType.Osu; }
            set
            {
                if (value && _currentThemeType != ThemeType.Osu)
                {
                    _currentThemeType = ThemeType.Osu;
                    RaisePropertyChanged();
                    RaisePropertyChanged(nameof(IsLightTheme));
                    RaisePropertyChanged(nameof(IsDarkTheme));
                    ApplyTheme();
                }
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
                if (_osuFolderPath == value)
                {
                    return;
                }
                _osuFolderPath = value;
                RaisePropertyChanged();
            }
        }


        #region 関数
        
        /// <summary>
        /// ViewModelを初期化します
        /// </summary>
        public void Initialize()
        {
            // 現在のテーマタイプを取得
            try
            {
                if (App.ThemeDomain?.CurrentTheme != null)
                {
                    _currentThemeType = App.ThemeDomain.CurrentTheme.CurrentThemeType;
                }
                else
                {
                    _currentThemeType = ThemeType.Light;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to get current theme: {ex.Message}");
                _currentThemeType = ThemeType.Light;
            }
            
            RaisePropertyChanged(nameof(IsLightTheme));
            RaisePropertyChanged(nameof(IsDarkTheme));
            RaisePropertyChanged(nameof(IsOsuTheme));
            
            // 設定からosu!フォルダパスを読み込み
            LoadSettings();
        }

        /// <summary>
        /// 設定を読み込みます
        /// </summary>
        private void LoadSettings()
        {
            try
            {
                // ConfigManager から読み込み
                OsuFolderPath = ConfigManager.CurrentConfig.OsuFolderPath ?? string.Empty;
            }
            catch
            {
                OsuFolderPath = string.Empty;
            }
        }

        /// <summary>
        /// 設定を保存します
        /// </summary>
        private void SaveSettings()
        {
            try
            {
                ConfigManager.UpdateOsuFolderPath(OsuFolderPath ?? string.Empty);
            }
            catch
            {
                // 設定保存に失敗した場合は無視
            }
        }

        /// <summary>
        /// 適用コマンド
        /// </summary>
        public ViewModelCommand ApplyCommand
        {
            get
            {
                return _applyCommand ??
                    (_applyCommand = new ViewModelCommand(Apply));
            }
        }
        
        #endregion

        /// <summary>
        /// OKコマンド
        /// </summary>
        public ViewModelCommand OkCommand
        {
            get
            {
                return _okCommand ??
                    (_okCommand = new ViewModelCommand(Ok));
            }
        }

        /// <summary>
        /// キャンセルコマンド
        /// </summary>
        public ViewModelCommand CancelCommand
        {
            get
            {
                return _cancelCommand ??
                    (_cancelCommand = new ViewModelCommand(Cancel));
            }
        }

        /// <summary>
        /// フォルダ選択コマンド
        /// </summary>
        public ViewModelCommand SelectFolderCommand
        {
            get
            {
                return _selectFolderCommand ??
                    (_selectFolderCommand = new ViewModelCommand(SelectFolder));
            }
        }

        /// <summary>
        /// 設定を適用します
        /// </summary>
        private void Apply()
        {
            // テーマ変更は既にリアルタイムで実行されている
            // 設定を保存
            SaveSettings();
        }

        /// <summary>
        /// テーマを適用します
        /// </summary>
        private void ApplyTheme()
        {
            if (App.ThemeDomain != null)
            {
                App.ThemeDomain.SetTheme(_currentThemeType);
                App.ThemeDomain.ApplyThemeToApplication();
            }
        }

        /// <summary>
        /// OKボタンの処理
        /// </summary>
        private void Ok()
        {
            Apply();
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
        }

        /// <summary>
        /// キャンセルボタンの処理
        /// </summary>
        private void Cancel()
        {
            // 元のテーマに戻す
            if (App.ThemeDomain?.CurrentTheme != null)
            {
                var originalThemeType = App.ThemeDomain.CurrentTheme.CurrentThemeType;
                if (_currentThemeType != originalThemeType)
                {
                    App.ThemeDomain.SetTheme(originalThemeType);
                    App.ThemeDomain.ApplyThemeToApplication();
                }
            }
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
        }

        /// <summary>
        /// フォルダを選択します
        /// </summary>
        private void SelectFolder()
        {
            var description = LanguageManager.GetString("SelectOsuFolderDialog");
            if (string.IsNullOrEmpty(description) || description == "SelectOsuFolderDialog")
            {
                description = Application.Current?.Resources["SelectOsuFolderDialog"] as string ?? "osu!フォルダを選択してください";
            }
            
            var dialog = new System.Windows.Forms.FolderBrowserDialog
            {
                Description = description
            };

            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                var selectedPath = dialog.SelectedPath;
                
                // 選択されたフォルダが有効なosu!フォルダかどうかを検証
                if (ValidateOsuFolder(selectedPath))
                {
                    OsuFolderPath = selectedPath;
                }
            }
        }

        /// <summary>
        /// osu!フォルダの有効性を検証します
        /// </summary>
        /// <param name="folderPath">検証するフォルダパス</param>
        /// <returns>有効なosu!フォルダの場合はtrue</returns>
        private bool ValidateOsuFolder(string folderPath)
        {
            try
            {
                if (string.IsNullOrEmpty(folderPath) || Directory.Exists(folderPath) == false)
                {
                    return false;
                }

                var missingFiles = new List<string>();
                
                // collection.dbの存在確認
                var collectionDbPath = Path.Combine(folderPath, "collection.db");
                if (File.Exists(collectionDbPath) == false)
                {
                    missingFiles.Add("collection.db");
                }

                // osu!.dbの存在確認
                var osuDbPath = Path.Combine(folderPath, "osu!.db");
                if (File.Exists(osuDbPath) == false)
                {
                    missingFiles.Add("osu!.db");
                }

                // 必要なファイルが見つからない場合はエラー表示
                if (missingFiles.Count > 0)
                {
                    ShowValidationError(missingFiles);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                ShowGeneralValidationError(ex);
                return false;
            }
        }

        /// <summary>
        /// フォルダ検証エラーを表示します
        /// </summary>
        /// <param name="missingFiles">見つからないファイルのリスト</param>
        private void ShowValidationError(List<string> missingFiles)
        {
            var messageTemplate = LanguageManager.GetString("InvalidOsuFolder");
            if (string.IsNullOrEmpty(messageTemplate) || messageTemplate == "InvalidOsuFolder")
            {
                messageTemplate = Application.Current?.Resources["InvalidOsuFolder"] as string ?? 
                    "選択されたフォルダは有効なosu!フォルダではありません。\n\n必要なファイルが見つかりません:\n{0}";
            }

            // \n を実際の改行文字に変換
            messageTemplate = messageTemplate.Replace("\\n", "\n");
            
            var missingFilesList = string.Join("\n", missingFiles.Select(f => "• " + f));
            var errorMessage = string.Format(messageTemplate, missingFilesList);

            var errorTitle = LanguageManager.GetString("FolderValidationFailed");
            if (string.IsNullOrEmpty(errorTitle) || errorTitle == "FolderValidationFailed")
            {
                errorTitle = Application.Current?.Resources["FolderValidationFailed"] as string ?? "フォルダの検証に失敗しました";
            }

            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        /// <summary>
        /// 一般的なフォルダ検証エラーを表示します
        /// </summary>
        /// <param name="ex">発生した例外</param>
        private void ShowGeneralValidationError(Exception ex)
        {
            var errorTitle = LanguageManager.GetString("FolderValidationFailed");
            if (string.IsNullOrEmpty(errorTitle) || errorTitle == "FolderValidationFailed")
            {
                errorTitle = Application.Current?.Resources["FolderValidationFailed"] as string ?? "フォルダの検証に失敗しました";
            }

            var errorPrefix = LanguageManager.GetString("Error");
            if (string.IsNullOrEmpty(errorPrefix) || errorPrefix == "Error")
            {
                errorPrefix = Application.Current?.Resources["Error"] as string ?? "エラー";
            }

            var errorMessage = $"{errorPrefix}: {ex.Message}";
            MessageBox.Show(errorMessage, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
        }
        
        #endregion
        
        #region メンバ変数
        
        /// <summary>
        /// 適用コマンドの実体
        /// </summary>
        private ViewModelCommand _applyCommand;
        
        /// <summary>
        /// OKコマンドの実体
        /// </summary>
        private ViewModelCommand _okCommand;
        
        /// <summary>
        /// キャンセルコマンドの実体
        /// </summary>
        private ViewModelCommand _cancelCommand;

        /// <summary>
        /// フォルダ選択コマンドの実体
        /// </summary>
        private ViewModelCommand _selectFolderCommand;
        
        #endregion
    }
}