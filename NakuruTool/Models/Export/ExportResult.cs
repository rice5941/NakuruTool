using System;
using System.Collections.Generic;

namespace NakuruTool.Models.Export
{
    /// <summary>
    /// エクスポート結果を格納するクラス
    /// </summary>
    public class ExportResult
    {
        #region コンストラクタ

        /// <summary>
        /// ExportResultクラスの新しいインスタンスを初期化します
        /// </summary>
        public ExportResult()
        {
            ExportedFiles = new List<string>();
            Errors = new List<string>();
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// エクスポートが成功したかどうか
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// エクスポートされたファイルパスのリスト
        /// </summary>
        public List<string> ExportedFiles { get; set; }

        /// <summary>
        /// エクスポートされたコレクション数
        /// </summary>
        public int ExportedCollectionCount { get; set; }

        /// <summary>
        /// エラーメッセージのリスト
        /// </summary>
        public List<string> Errors { get; set; }

        /// <summary>
        /// エクスポート開始時刻
        /// </summary>
        public DateTime StartTime { get; set; }

        /// <summary>
        /// エクスポート終了時刻
        /// </summary>
        public DateTime EndTime { get; set; }

        /// <summary>
        /// エクスポートに要した時間
        /// </summary>
        public TimeSpan Duration
        {
            get { return EndTime - StartTime; }
        }

        /// <summary>
        /// 出力フォルダパス
        /// </summary>
        public string OutputFolderPath { get; set; } = string.Empty;

        #endregion

        #region 関数

        /// <summary>
        /// エクスポートファイルを追加します
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        public void AddExportedFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath) == false)
            {
                ExportedFiles.Add(filePath);
            }
        }

        /// <summary>
        /// エラーメッセージを追加します
        /// </summary>
        /// <param name="errorMessage">エラーメッセージ</param>
        public void AddError(string errorMessage)
        {
            if (string.IsNullOrEmpty(errorMessage) == false)
            {
                Errors.Add(errorMessage);
            }
        }

        #endregion
    }
}