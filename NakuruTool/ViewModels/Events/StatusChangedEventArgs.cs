using System;

namespace NakuruTool.ViewModels.Events
{
    /// <summary>
    /// ステータス変更イベント引数
    /// </summary>
    public class StatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// ローディング状態
        /// </summary>
        public bool IsLoading { get; set; }
        
        /// <summary>
        /// ステータスメッセージ
        /// </summary>
        public string StatusMessage { get; set; }
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        /// <param name="isLoading">ローディング状態</param>
        /// <param name="statusMessage">ステータスメッセージ</param>
        public StatusChangedEventArgs(bool isLoading, string statusMessage)
        {
            IsLoading = isLoading;
            StatusMessage = statusMessage;
        }
    }
}