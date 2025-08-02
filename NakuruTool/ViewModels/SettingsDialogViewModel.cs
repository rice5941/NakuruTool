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
        /// ダークテーマが選択されているかどうか
        /// </summary>
        private bool _isDarkTheme;
        
        #endregion

        #region プロパティ
        
        /// <summary>
        /// ダークテーマが選択されているかどうか
        /// </summary>
        public bool IsDarkTheme
        {
            get { return _isDarkTheme; }
            set
            {
                if (_isDarkTheme == value)
                {
                    return;
                }
                _isDarkTheme = value;
                RaisePropertyChanged();
                
                // リアルタイムでテーマを変更
                if (App.IsDarkTheme != _isDarkTheme)
                {
                    App.SwitchTheme();
                }
            }
        }


        #region 関数
        
        /// <summary>
        /// ViewModelを初期化します
        /// </summary>
        public void Initialize()
        {
            IsDarkTheme = App.IsDarkTheme;
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
        /// 設定を適用します
        /// </summary>
        private void Apply()
        {
            // テーマ変更は既にリアルタイムで実行されているため、ここでは何もしない
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
            IsDarkTheme = App.IsDarkTheme;
            Messenger.Raise(new WindowActionMessage(WindowAction.Close, "Close"));
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
        
        #endregion
    }
}