using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OsuParsers.Database;
using OsuParsers.Decoders;
using OsuParsers.Database.Objects;
using NakuruTool.Models.Collection;
using NakuruTool.Services;

namespace NakuruTool.Models.Export
{
    /// <summary>
    /// エクスポート処理を行うクラス
    /// </summary>
    public class ExportStore
    {
        #region 関数

        /// <summary>
        /// 指定されたコレクションをJSONファイルにエクスポートします
        /// </summary>
        /// <param name="collections">エクスポートするコレクションのリスト</param>
        /// <param name="osuFolderPath">osu!フォルダのパス</param>
        /// <param name="outputFolderPath">出力フォルダのパス</param>
        /// <param name="progressCallback">進行状況コールバック</param>
        /// <returns>エクスポート結果</returns>
        public async Task<ExportResult> ExportCollectionsAsync(
            List<Collection.Collection> collections, 
            string osuFolderPath, 
            string outputFolderPath,
            IProgress<string> progressCallback = null)
        {
            var result = new ExportResult
            {
                StartTime = DateTime.Now,
                OutputFolderPath = outputFolderPath
            };

            try
            {
                // 入力チェック
                if (ValidateInput(collections, osuFolderPath, outputFolderPath, result) == false)
                {
                    result.EndTime = DateTime.Now;
                    return result;
                }

                // 出力フォルダを作成
                progressCallback?.Report(LanguageManager.GetString("CreatingOutputFolder") ?? "出力フォルダを作成中...");
                EnsureOutputFolder(outputFolderPath);

                // osu!.dbを読み込み（ビートマップ詳細情報取得用）
                progressCallback?.Report(LanguageManager.GetString("LoadingBeatmapDatabase") ?? "ビートマップデータベースを読み込み中...");
                var beatmapInfos = await LoadBeatmapDatabase(osuFolderPath);

                // 各コレクションを個別ファイルとしてエクスポート
                progressCallback?.Report(LanguageManager.GetString("ExportingCollections") ?? "コレクションをエクスポート中...");
                
                var exportTasks = collections.Select(async collection =>
                {
                    try
                    {
                        var outputPath = Path.Combine(outputFolderPath, SanitizeFileName(collection.Name) + ".json");
                        await ExportSingleCollectionAsync(collection, beatmapInfos, outputPath);
                        result.AddExportedFile(outputPath);
                        return true;
                    }
                    catch (Exception ex)
                    {
                        var errorMessage = string.Format(
                            LanguageManager.GetString("ErrorExportingCollection") ?? "コレクション '{0}' のエクスポートに失敗: {1}",
                            collection.Name, ex.Message);
                        result.AddError(errorMessage);
                        return false;
                    }
                });

                var exportResults = await Task.WhenAll(exportTasks);
                
                result.ExportedCollectionCount = exportResults.Count(r => r);
                result.IsSuccess = result.ExportedCollectionCount > 0;
                
                progressCallback?.Report(LanguageManager.GetString("ExportCompleted") ?? "エクスポートが完了しました");
            }
            catch (Exception ex)
            {
                result.AddError(ex.Message);
                result.IsSuccess = false;
            }
            finally
            {
                result.EndTime = DateTime.Now;
            }

            return result;
        }

        /// <summary>
        /// 入力パラメータを検証します
        /// </summary>
        /// <param name="collections">コレクションリスト</param>
        /// <param name="osuFolderPath">osu!フォルダパス</param>
        /// <param name="outputFolderPath">出力フォルダパス</param>
        /// <param name="result">結果オブジェクト</param>
        /// <returns>検証成功の場合true</returns>
        private bool ValidateInput(List<Collection.Collection> collections, string osuFolderPath, 
            string outputFolderPath, ExportResult result)
        {
            if (collections == null || collections.Count == 0)
            {
                result.AddError(LanguageManager.GetString("NoCollectionsToExport") ?? "エクスポートするコレクションがありません");
                return false;
            }

            if (string.IsNullOrWhiteSpace(osuFolderPath))
            {
                result.AddError(LanguageManager.GetString("OsuFolderPathNotSpecified") ?? "osu!フォルダパスが指定されていません");
                return false;
            }

            if (Directory.Exists(osuFolderPath) == false)
            {
                result.AddError(string.Format(
                    LanguageManager.GetString("OsuFolderNotFound") ?? "osu!フォルダが見つかりません: {0}",
                    osuFolderPath));
                return false;
            }

            var osuDbPath = Path.Combine(osuFolderPath, "osu!.db");
            if (File.Exists(osuDbPath) == false)
            {
                result.AddError(string.Format(
                    LanguageManager.GetString("OsuDbNotFound") ?? "osu!.dbファイルが見つかりません: {0}",
                    osuDbPath));
                return false;
            }

            if (string.IsNullOrWhiteSpace(outputFolderPath))
            {
                result.AddError(LanguageManager.GetString("OutputFolderPathNotSpecified") ?? "出力フォルダパスが指定されていません");
                return false;
            }

            return true;
        }

        /// <summary>
        /// 出力フォルダを確保します
        /// </summary>
        /// <param name="outputFolderPath">出力フォルダパス</param>
        private void EnsureOutputFolder(string outputFolderPath)
        {
            if (Directory.Exists(outputFolderPath) == false)
            {
                Directory.CreateDirectory(outputFolderPath);
            }
        }

        /// <summary>
        /// osu!.dbからビートマップデータベースを読み込みます
        /// </summary>
        /// <param name="osuFolderPath">osu!フォルダパス</param>
        /// <returns>MD5ハッシュをキーとしたビートマップ情報の辞書</returns>
        private async Task<ConcurrentDictionary<string, DbBeatmap>> LoadBeatmapDatabase(string osuFolderPath)
        {
            var osuDbPath = Path.Combine(osuFolderPath, "osu!.db");
            
            return await Task.Run(() =>
            {
                var osuDatabase = DatabaseDecoder.DecodeOsu(osuDbPath);
                var beatmapInfos = new ConcurrentDictionary<string, DbBeatmap>(
                    Environment.ProcessorCount,
                    osuDatabase.Beatmaps.Count);

                Parallel.ForEach(osuDatabase.Beatmaps, new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                }, beatmap =>
                {
                    if (string.IsNullOrEmpty(beatmap.MD5Hash) == false)
                    {
                        beatmapInfos.TryAdd(beatmap.MD5Hash, beatmap);
                    }
                });

                return beatmapInfos;
            });
        }

        /// <summary>
        /// 単一のコレクションをJSONファイルにエクスポートします
        /// </summary>
        /// <param name="collection">エクスポートするコレクション</param>
        /// <param name="beatmapInfos">ビートマップ詳細情報の辞書</param>
        /// <param name="outputPath">出力ファイルパス</param>
        private async Task ExportSingleCollectionAsync(Collection.Collection collection, 
            ConcurrentDictionary<string, DbBeatmap> beatmapInfos, string outputPath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 65536))
            using (var writer = new Utf8JsonWriter(fileStream, new JsonWriterOptions
            {
                Indented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }))
            {
                writer.WriteStartArray();
                writer.WriteStartObject();
                writer.WriteString("name", collection.Name);
                writer.WritePropertyName("beatmaps");
                writer.WriteStartArray();

                // ビートマップを並列処理でJSONに変換
                const int batchSize = 1000;
                for (int i = 0; i < collection.Beatmaps.Count; i += batchSize)
                {
                    var batch = collection.Beatmaps.Skip(i).Take(batchSize).ToList();
                    var beatmapBatch = await ProcessBeatmapsAsync(batch, beatmapInfos);

                    foreach (var beatmap in beatmapBatch)
                    {
                        JsonSerializer.Serialize(writer, beatmap, options);
                    }

                    // メモリ最適化のためのガベージコレクション
                    if (i % 5000 == 0 && i > 0)
                    {
                        GC.Collect(0, GCCollectionMode.Optimized);
                    }
                }

                writer.WriteEndArray();
                writer.WriteEndObject();
                writer.WriteEndArray();
                await writer.FlushAsync();
            }
        }

        /// <summary>
        /// ビートマップリストを並列処理でJSON用オブジェクトに変換します
        /// </summary>
        /// <param name="beatmaps">ビートマップリスト</param>
        /// <param name="beatmapInfos">ビートマップ詳細情報の辞書</param>
        /// <returns>JSON用オブジェクトの配列</returns>
        private async Task<object[]> ProcessBeatmapsAsync(List<Beatmap> beatmaps, 
            ConcurrentDictionary<string, DbBeatmap> beatmapInfos)
        {
            return await Task.Run(() =>
            {
                return beatmaps.AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .Select(beatmap =>
                    {
                        var beatmapInfo = beatmapInfos.TryGetValue(beatmap.Md5Hash, out var info) ? info : null;
                        var unknownText = LanguageManager.GetString("Unknown") ?? "Unknown";
                        
                        return new
                        {
                            md5 = beatmap.Md5Hash,
                            title = beatmapInfo?.Title ?? beatmap.Title ?? unknownText,
                            artist = beatmapInfo?.Artist ?? beatmap.Artist ?? unknownText,
                            version = beatmapInfo?.Difficulty ?? beatmap.Version ?? unknownText,
                            creator = beatmapInfo?.Creator ?? beatmap.Creator ?? unknownText,
                            cs = (double)(beatmapInfo?.CircleSize ?? 0.0f),
                            mode = GetGameModeName(beatmapInfo?.Ruleset ?? OsuParsers.Enums.Ruleset.Standard),
                            status = GetRankedStatusName(beatmapInfo?.RankedStatus),
                            beatmapset_id = beatmapInfo?.BeatmapSetId ?? 0,
                            beatmap_id = beatmapInfo?.BeatmapId ?? 0
                        };
                    })
                    .ToArray();
            });
        }

        /// <summary>
        /// ファイル名として無効な文字を安全な文字に置換します
        /// </summary>
        /// <param name="fileName">元のファイル名</param>
        /// <returns>安全なファイル名</returns>
        private string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "Unknown";
            }

            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = fileName;

            foreach (var invalidChar in invalidChars)
            {
                sanitized = sanitized.Replace(invalidChar, '_');
            }

            sanitized = sanitized
                .Replace(":", "_")
                .Replace("?", "_")
                .Replace("*", "_")
                .Replace("\"", "_")
                .Replace("<", "_")
                .Replace(">", "_")
                .Replace("|", "_")
                .Replace("/", "_")
                .Replace("\\", "_")
                .Trim()
                .Trim('.');

            if (string.IsNullOrEmpty(sanitized) || sanitized == "." || sanitized == "..")
            {
                return "Unknown";
            }

            if (sanitized.Length > 200)
            {
                sanitized = sanitized.Substring(0, 200);
            }

            return sanitized;
        }

        /// <summary>
        /// OsuParsersのRulesetを文字列に変換します
        /// </summary>
        /// <param name="ruleset">ルールセット</param>
        /// <returns>ゲームモード名</returns>
        private string GetGameModeName(OsuParsers.Enums.Ruleset ruleset)
        {
            return ruleset switch
            {
                OsuParsers.Enums.Ruleset.Standard => "Standard",
                OsuParsers.Enums.Ruleset.Taiko => "Taiko",
                OsuParsers.Enums.Ruleset.Fruits => "Catch",
                OsuParsers.Enums.Ruleset.Mania => "Mania",
                _ => "Unknown"
            };
        }

        /// <summary>
        /// ランクステータスを文字列に変換します
        /// </summary>
        /// <param name="rankedStatus">ランクステータス</param>
        /// <returns>ランクステータス名</returns>
        private string GetRankedStatusName(object rankedStatus)
        {
            if (rankedStatus == null)
            {
                return "Unknown";
            }

            var statusName = rankedStatus.ToString() ?? "Unknown";
            return statusName switch
            {
                "Unknown" => "Unknown",
                "Unsubmitted" => "Unsubmitted",
                "Pending" => "Pending/WIP/Graveyard",
                "Ranked" => "Ranked",
                "Approved" => "Approved",
                "Qualified" => "Qualified",
                "Loved" => "Loved",
                _ => statusName
            };
        }

        #endregion
    }
}