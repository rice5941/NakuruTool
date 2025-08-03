using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NakuruTool.Models.Collection;
using NakuruTool.Services;

namespace NakuruTool.Models.Export
{
    /// <summary>
    /// エクスポート設定のデータ実体を管理するドメインクラス
    /// </summary>
    public class ExportDomain
    {
        #region コンストラクタ

        /// <summary>
        /// ExportDomainクラスの新しいインスタンスを初期化します
        /// </summary>
        public ExportDomain()
        {
            _exportStore = new ExportStore();
        }

        /// <summary>
        /// 指定されたExportStoreでExportDomainクラスのインスタンスを初期化します
        /// </summary>
        /// <param name="exportStore">エクスポートストア</param>
        public ExportDomain(ExportStore exportStore)
        {
            _exportStore = exportStore ?? throw new ArgumentNullException(nameof(exportStore));
        }

        #endregion

        #region 関数

        /// <summary>
        /// 指定されたコレクションをエクスポートします
        /// </summary>
        /// <param name="collections">エクスポートするコレクションのリスト</param>
        /// <param name="osuFolderPath">osu!フォルダのパス</param>
        /// <param name="progressCallback">進行状況コールバック</param>
        /// <returns>エクスポート結果</returns>
        public async Task<ExportResult> ExportCollectionsAsync(List<Collection.Collection> collections, 
            string osuFolderPath, IProgress<string> progressCallback = null)
        {
            if (collections == null)
            {
                throw new ArgumentNullException(nameof(collections));
            }

            if (string.IsNullOrEmpty(osuFolderPath))
            {
                var message = LanguageManager.GetString("OsuFolderPathNotSpecified") ?? "osu!フォルダパスが指定されていません";
                throw new ArgumentException(message, nameof(osuFolderPath));
            }

            var outputFolderPath = GetDefaultExportFolderPath();
            return await _exportStore.ExportCollectionsAsync(collections, osuFolderPath, outputFolderPath, progressCallback);
        }

        /// <summary>
        /// 指定されたコレクションをカスタム出力フォルダにエクスポートします
        /// </summary>
        /// <param name="collections">エクスポートするコレクションのリスト</param>
        /// <param name="osuFolderPath">osu!フォルダのパス</param>
        /// <param name="outputFolderPath">出力フォルダのパス</param>
        /// <param name="progressCallback">進行状況コールバック</param>
        /// <returns>エクスポート結果</returns>
        public async Task<ExportResult> ExportCollectionsToFolderAsync(List<Collection.Collection> collections, 
            string osuFolderPath, string outputFolderPath, IProgress<string> progressCallback = null)
        {
            if (collections == null)
            {
                throw new ArgumentNullException(nameof(collections));
            }

            if (string.IsNullOrEmpty(osuFolderPath))
            {
                var message = LanguageManager.GetString("OsuFolderPathNotSpecified") ?? "osu!フォルダパスが指定されていません";
                throw new ArgumentException(message, nameof(osuFolderPath));
            }

            if (string.IsNullOrEmpty(outputFolderPath))
            {
                var message = LanguageManager.GetString("OutputFolderPathNotSpecified") ?? "出力フォルダパスが指定されていません";
                throw new ArgumentException(message, nameof(outputFolderPath));
            }

            return await _exportStore.ExportCollectionsAsync(collections, osuFolderPath, outputFolderPath, progressCallback);
        }

        /// <summary>
        /// エクスポート可能かどうかを判定します
        /// </summary>
        /// <param name="collections">コレクションのリスト</param>
        /// <param name="osuFolderPath">osu!フォルダのパス</param>
        /// <returns>エクスポート可能な場合はtrue</returns>
        public bool CanExport(List<Collection.Collection> collections, string osuFolderPath)
        {
            // コレクションが存在するか
            if (collections == null || collections.Count == 0)
            {
                return false;
            }

            // 選択されたコレクションが存在するか
            if (collections.Any(c => c != null && string.IsNullOrEmpty(c.Name) == false) == false)
            {
                return false;
            }

            // osu!フォルダが存在するか
            if (string.IsNullOrWhiteSpace(osuFolderPath) || Directory.Exists(osuFolderPath) == false)
            {
                return false;
            }

            // osu!.dbファイルが存在するか
            var osuDbPath = Path.Combine(osuFolderPath, "osu!.db");
            if (File.Exists(osuDbPath) == false)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 選択されたコレクションの数を取得します
        /// </summary>
        /// <param name="collections">コレクションのリスト</param>
        /// <returns>選択されたコレクション数</returns>
        public int GetSelectedCollectionCount(List<Collection.Collection> collections)
        {
            if (collections == null)
            {
                return 0;
            }

            return collections.Count(c => c != null && string.IsNullOrEmpty(c.Name) == false);
        }

        /// <summary>
        /// デフォルトのエクスポートフォルダパスを取得します
        /// </summary>
        /// <returns>デフォルトのエクスポートフォルダパス</returns>
        public string GetDefaultExportFolderPath()
        {
            try
            {
                // 実行ファイルと同階層のExportフォルダ
                var executablePath = AppDomain.CurrentDomain.BaseDirectory;
                return Path.Combine(executablePath, "Export");
            }
            catch
            {
                // フォールバック: カレントディレクトリのExportフォルダ
                return Path.Combine(Directory.GetCurrentDirectory(), "Export");
            }
        }

        /// <summary>
        /// エクスポートフォルダが書き込み可能かどうかを確認します
        /// </summary>
        /// <param name="folderPath">フォルダパス</param>
        /// <returns>書き込み可能な場合はtrue</returns>
        public bool IsExportFolderWritable(string folderPath = null)
        {
            try
            {
                var targetPath = folderPath ?? GetDefaultExportFolderPath();
                
                // フォルダが存在しない場合は作成を試みる
                if (Directory.Exists(targetPath) == false)
                {
                    Directory.CreateDirectory(targetPath);
                }

                // テストファイルの書き込みを試行
                var testFilePath = Path.Combine(targetPath, $"test_{Guid.NewGuid()}.tmp");
                File.WriteAllText(testFilePath, "test");
                File.Delete(testFilePath);
                
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// エクスポート結果の概要メッセージを生成します
        /// </summary>
        /// <param name="result">エクスポート結果</param>
        /// <returns>概要メッセージ</returns>
        public string GenerateSummaryMessage(ExportResult result)
        {
            if (result == null)
            {
                return LanguageManager.GetString("ExportResultNotAvailable") ?? "エクスポート結果が利用できません";
            }

            if (result.IsSuccess)
            {
                var successTemplate = LanguageManager.GetString("ExportSuccessMessage") ?? 
                    "エクスポートが完了しました。\n\n" +
                    "エクスポートされたコレクション数: {0}\n" +
                    "出力フォルダ: {1}\n" +
                    "処理時間: {2:F2}秒";

                // \n を実際の改行文字に変換
                successTemplate = successTemplate.Replace("\\n", "\n");

                return string.Format(successTemplate, 
                    result.ExportedCollectionCount, 
                    result.OutputFolderPath, 
                    result.Duration.TotalSeconds);
            }
            else
            {
                var errorTemplate = LanguageManager.GetString("ExportErrorMessage") ?? 
                    "エクスポートでエラーが発生しました。\n\n" +
                    "エラー数: {0}\n" +
                    "最初のエラー: {1}";

                // \n を実際の改行文字に変換
                errorTemplate = errorTemplate.Replace("\\n", "\n");

                var firstError = result.Errors.FirstOrDefault() ?? "不明なエラー";
                return string.Format(errorTemplate, result.Errors.Count, firstError);
            }
        }

        #endregion

        #region メンバ変数

        /// <summary>
        /// エクスポートストア
        /// </summary>
        private readonly ExportStore _exportStore;

        #endregion
    }
}