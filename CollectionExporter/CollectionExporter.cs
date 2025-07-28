using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OsuParsers.Database;
using OsuParsers.Decoders;

namespace CollectionExporter
{
    /// <summary>
    /// osu! collection.dbファイルを読み込み、JSONフォーマットに変換するコンソールアプリケーション
    /// </summary>
    class Program
    {
        
        static async Task Main(string[] args)
        {
            
            var totalStopwatch = Stopwatch.StartNew();
            
            try
            {
                Console.WriteLine("=== osu! Collection Exporter ===");
                Console.WriteLine();
                
                // ライセンス情報表示
                ShowLicenseInfo();
                
                // 設定ファイルから設定を読み込み
                var config = LoadConfig("config.json");
                
                // 設定内容を表示
                ShowConfigInfo(config);
                
                // OsuFolderPathの存在チェック
                if (!ValidateOsuFolderPath(config.OsuFolderPath))
                {
                    Console.WriteLine("Press Enter to exit...");
                    Console.ReadLine();
                    Environment.Exit(1);
                }
                
                // 引数なしの場合のみEnterキー待機
                if (args.Length == 0)
                {
                    Console.WriteLine("Press Enter to continue...");
                    Console.ReadLine();
                    Console.WriteLine();
                }
                
                // 引数とconfig.jsonの処理分離
                List<string>? targetCollectionNames = null;
                
                if (args.Length > 0)
                {
                    // 引数指定の場合：引数に従って実行
                    targetCollectionNames = args.ToList();
                    Console.WriteLine($"引数で指定されたコレクション（{args.Length}個）: {string.Join(", ", args)}");
                }
                else
                {
                    // 引数なしの場合：config.jsonに従って実行
                    if (config.TargetCollections != null && config.TargetCollections.Count > 0)
                    {
                        targetCollectionNames = config.TargetCollections;
                        Console.WriteLine($"config.jsonで指定されたコレクション（{config.TargetCollections.Count}個）: {string.Join(", ", targetCollectionNames)}");
                    }
                    else
                    {
                        Console.WriteLine("config.jsonに設定がありません。全コレクションを処理します。");
                        Console.WriteLine("特定のコレクションのみ処理する場合は引数で指定するか、config.jsonを設定してください。");
                    }
                }
                
                // osu!フォルダ内のファイルパスを構築
                var collectionDbPath = Path.Combine(config.OsuFolderPath, "collection.db");
                var osuDbPath = Path.Combine(config.OsuFolderPath, "osu!.db");
                
                // 並列でファイル読み込みを開始
                var collectionTask = Task.Run(() => ReadCollectionDb(collectionDbPath));
                var osuDbTask = Task.Run(() => ReadOsuDbWithOsuParsersParallel(osuDbPath));
                
                // collection.db読み込み完了を待機
                var collections = await collectionTask;
                
                // osu!.db読み込み完了を待機
                var fullBeatmapInfos = await osuDbTask;
                
                // 軽量化されたビートマップ情報に変換
                var beatmapInfos = ConvertToOptimizedBeatmapInfos(fullBeatmapInfos);
                
                // フルサイズのデータを明示的に破棄
                fullBeatmapInfos.Clear();
                GC.Collect(2, GCCollectionMode.Forced);
                
                Console.WriteLine($"最適化されたビートマップ情報: {beatmapInfos.Count}個");
                
                // 出力フォルダを準備
                var outputFolder = config.OutputFolder ?? "output";
                if (!Directory.Exists(outputFolder))
                {
                    Directory.CreateDirectory(outputFolder);
                }

                List<Collection> filteredCollections;
                
                if (targetCollectionNames != null && targetCollectionNames.Count > 0)
                {
                    // コレクション指定
                    filteredCollections = new List<Collection>();
                    var notFound = new List<string>();
                    
                    foreach (var targetName in targetCollectionNames)
                    {
                        var found = FilterCollections(collections, targetName);
                        if (found.Count > 0)
                        {
                            filteredCollections.AddRange(found);
                        }
                        else
                        {
                            notFound.Add(targetName);
                        }
                    }
                    
                    if (notFound.Count > 0)
                    {
                        Console.WriteLine($"見つからなかったコレクション: {string.Join(", ", notFound)}");
                    }
                    
                    if (filteredCollections.Count == 0)
                    {
                        Console.WriteLine("指定されたコレクションがすべて見つかりませんでした。");
                        Console.WriteLine("利用可能なコレクション:");
                        foreach (var collection in collections)
                        {
                            Console.WriteLine($"  - {collection.Name}");
                        }
                        Environment.Exit(1);
                    }
                    
                    Console.WriteLine($"処理対象: {filteredCollections.Count}個のコレクション");
                }
                else
                {
                    // 全コレクション
                    filteredCollections = collections;
                }

                // コレクションを個別ファイルとして出力
                Console.WriteLine("各コレクションを個別ファイルとして出力中...");
                
                var outputTasks = filteredCollections.Select(async collection =>
                {
                    var singleCollectionList = new List<Collection> { collection };
                    var outputPath = Path.Combine(outputFolder, SanitizeFileName(collection.Name) + ".json");
                    await WriteToJsonParallel(singleCollectionList, beatmapInfos, outputPath);
                    return outputPath;
                });
                
                var outputPaths = await Task.WhenAll(outputTasks);
                
                totalStopwatch.Stop();
                Console.WriteLine($"変換完了: {filteredCollections.Count}個のコレクションを個別ファイルとして出力しました。");
                Console.WriteLine($"出力フォルダ: {outputFolder}");
                Console.WriteLine("出力ファイル:");
                foreach (var path in outputPaths)
                {
                    Console.WriteLine($"  - {Path.GetFileName(path)}");
                }
                Console.WriteLine($"総処理時間: {totalStopwatch.ElapsedMilliseconds:N0}ms ({totalStopwatch.Elapsed.TotalSeconds:F2}秒)");
                
                // 引数なしの場合のみEnterキー待機
                if (args.Length == 0)
                {
                    Console.WriteLine("\nPress Enter to exit...");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"エラーが発生しました: {ex.Message}");
                
                // 引数なしの場合のみEnterキー待機
                if (args.Length == 0)
                {
                    Console.WriteLine("Press Enter to exit...");
                    Console.ReadLine();
                }
                
                Environment.Exit(1);
            }
        }


        /// <summary>
        /// ライセンス情報を表示します
        /// </summary>
        static void ShowLicenseInfo()
        {
            Console.WriteLine("Third-party Libraries:");
            Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
            Console.WriteLine("│ OsuParsers v1.7.2                                              │");
            Console.WriteLine("│ Copyright (c) 2019 mrflashstudio (Mikhail Cherepanov)          │");
            Console.WriteLine("│ Licensed under the MIT License                                 │");
            Console.WriteLine("│ https://github.com/mrflashstudio/OsuParsers                    │");
            Console.WriteLine("├─────────────────────────────────────────────────────────────────┤");
            Console.WriteLine("│ System.Text.Json v8.0.5                                        │");
            Console.WriteLine("│ Copyright (c) Microsoft Corporation                            │");
            Console.WriteLine("│ Licensed under the MIT License                                 │");
            Console.WriteLine("│ https://github.com/dotnet/runtime                              │");
            Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");
            Console.WriteLine();
        }

        /// <summary>
        /// OsuFolderPathの有効性を検証します
        /// </summary>
        /// <param name="osuFolderPath">osu!フォルダのパス</param>
        /// <returns>有効な場合はtrue、無効な場合はfalse</returns>
        static bool ValidateOsuFolderPath(string osuFolderPath)
        {
            // パスが空または null の場合
            if (string.IsNullOrWhiteSpace(osuFolderPath))
            {
                Console.WriteLine("ERROR: osu! folder path is empty or null in config.json");
                Console.WriteLine("Please set the correct osu! folder path in config.json");
                return false;
            }
            
            // フォルダが存在しない場合
            if (!Directory.Exists(osuFolderPath))
            {
                Console.WriteLine($"ERROR: osu! folder not found: {osuFolderPath}");
                return false;
            }
            
            // collection.db の存在チェック
            var collectionDbPath = Path.Combine(osuFolderPath, "collection.db");
            if (!File.Exists(collectionDbPath))
            {
                Console.WriteLine($"WARNING: collection.db not found in: {osuFolderPath}");
                return false;
            }
            
            // osu!.db の存在チェック（必須）
            var osuDbPath = Path.Combine(osuFolderPath, "osu!.db");
            if (!File.Exists(osuDbPath))
            {
                Console.WriteLine($"ERROR: osu!.db not found in: {osuFolderPath}");
                return false;
            }

            Console.WriteLine();
            return true;
        }

        /// <summary>
        /// 設定内容を表示します
        /// </summary>
        /// <param name="config">設定情報</param>
        static void ShowConfigInfo(Config config)
        {
            Console.WriteLine("Configuration:");
            Console.WriteLine("┌─────────────────────────────────────────────────────────────────┐");
            Console.WriteLine($"│ osu! Folder Path: {config.OsuFolderPath,-42} │");
            Console.WriteLine($"│ Output Folder   : {config.OutputFolder ?? "output",-42} │");
            
            if (config.TargetCollections != null && config.TargetCollections.Count > 0)
            {
                Console.WriteLine($"│ Target Collections ({config.TargetCollections.Count} items):                               │");
                foreach (var collection in config.TargetCollections.Take(3))
                {
                    var displayName = collection.Length > 59 ? collection.Substring(0, 56) + "..." : collection;
                    Console.WriteLine($"│   - {displayName,-60} │");
                }
                if (config.TargetCollections.Count > 3)
                {
                    Console.WriteLine($"│   ... and {config.TargetCollections.Count - 3} more                                        │");
                }
            }
            else
            {
                Console.WriteLine($"│ Target Collections: All collections will be processed         │");
            }
            
            Console.WriteLine("└─────────────────────────────────────────────────────────────────┘");
            Console.WriteLine();
        }

        /// <summary>
        /// 設定ファイルを読み込みます
        /// </summary>
        /// <param name="configPath">設定ファイルのパス</param>
        /// <returns>設定情報</returns>
        static Config LoadConfig(string configPath)
        {
            if (!File.Exists(configPath))
            {
                // デフォルト設定を返す
                return new Config
                {
                    OsuFolderPath = "C:/Users/YourUsername/AppData/Local/osu!",
                    OutputFolder = "output",
                    TargetCollections = null
                };
            }
            
            var configJson = File.ReadAllText(configPath, Encoding.UTF8);
            return JsonSerializer.Deserialize<Config>(configJson) ?? new Config
            {
                OsuFolderPath = "C:/Users/YourUsername/AppData/Local/osu!",
                OutputFolder = "output",
                TargetCollections = null
            };
        }

        /// <summary>
        /// collection.dbファイルを読み込み、コレクション情報を取得します
        /// </summary>
        /// <param name="filePath">collection.dbファイルのパス</param>
        /// <returns>コレクション情報のリスト</returns>
        static List<Collection> ReadCollectionDb(string filePath)
        {
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"collection.dbファイルが見つかりません: {filePath}");
            }

            var collections = new List<Collection>();

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fileStream, Encoding.UTF8))
            {
                // バージョン情報を読み込み（4バイト）
                var version = reader.ReadInt32();
                Console.WriteLine($"collection.db バージョン: {version}");

                // コレクション数を読み込み（4バイト）
                var collectionCount = reader.ReadInt32();
                Console.WriteLine($"コレクション数: {collectionCount}");

                // 各コレクションを読み込み
                for (int i = 0; i < collectionCount; i++)
                {
                    var collection = new Collection();

                    // コレクション名を読み込み
                    collection.Name = ReadOsuString(reader);

                    // ビートマップ数を読み込み
                    var beatmapCount = reader.ReadInt32();

                    // MD5ハッシュリストを読み込み
                    collection.BeatmapMd5s = new List<string>();
                    for (int j = 0; j < beatmapCount; j++)
                    {
                        var md5Hash = ReadOsuString(reader);
                        collection.BeatmapMd5s.Add(md5Hash);
                    }

                    collections.Add(collection);
                    Console.WriteLine($"コレクション '{collection.Name}' を読み込み完了（{beatmapCount}個のビートマップ）");
                }
            }

            return collections;
        }

        /// <summary>
        /// osu!形式の文字列を読み込みます（ReadOnlySpan<T>最適化版）
        /// プレフィックスバイト + ULEB128長さ + UTF-8文字列
        /// </summary>
        /// <param name="reader">バイナリリーダー</param>
        /// <returns>読み込まれた文字列</returns>
        static string ReadOsuString(BinaryReader reader)
        {
            var prefix = reader.ReadByte();
            
            if (prefix == 0x00)
            {
                // 空文字列
                return string.Empty;
            }
            else if (prefix == 0x0b)
            {
                // 文字列あり
                var length = ReadULEB128(reader);
                var bytes = reader.ReadBytes((int)length);
                
                // ReadOnlySpan<T>を使用してメモリ効率を向上
                ReadOnlySpan<byte> byteSpan = bytes.AsSpan();
                return Encoding.UTF8.GetString(byteSpan);
            }
            else
            {
                throw new InvalidDataException($"無効な文字列プレフィックス: 0x{prefix:X2}");
            }
        }

        /// <summary>
        /// ULEB128形式の整数を読み込みます
        /// </summary>
        /// <param name="reader">バイナリリーダー</param>
        /// <returns>読み込まれた整数値</returns>
        static uint ReadULEB128(BinaryReader reader)
        {
            uint result = 0;
            int shift = 0;

            while (true)
            {
                var b = reader.ReadByte();
                result |= (uint)(b & 0x7F) << shift;

                if ((b & 0x80) == 0)
                    break;

                shift += 7;
            }

            return result;
        }

        /// <summary>
        /// OsuParsersを使用してosu!.dbファイルを並列で読み込み、ビートマップ情報を取得します
        /// </summary>
        /// <param name="filePath">osu!.dbファイルのパス</param>
        /// <returns>MD5ハッシュをキーとするビートマップ情報の辞書</returns>
        static ConcurrentDictionary<string, OsuParsers.Database.Objects.DbBeatmap> ReadOsuDbWithOsuParsersParallel(string filePath)
        {
            var stopwatch = Stopwatch.StartNew();
            Console.WriteLine("osu!.dbを読み込み中...");
            
            try
            {
                // OsuParsersを使用してosu!.dbを読み込み
                var osuDatabase = DatabaseDecoder.DecodeOsu(filePath);
                
                Console.WriteLine($"osu!.db バージョン: {osuDatabase.OsuVersion}");
                Console.WriteLine($"ビートマップ数: {osuDatabase.Beatmaps.Count}");

                // 並列処理でMD5ハッシュをキーとする辞書を作成（初期容量を最適化）
                var beatmapInfos = new ConcurrentDictionary<string, OsuParsers.Database.Objects.DbBeatmap>(
                    Environment.ProcessorCount, 
                    osuDatabase.Beatmaps.Count);
                
                Parallel.ForEach(osuDatabase.Beatmaps, new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                }, beatmap =>
                {
                    if (!string.IsNullOrEmpty(beatmap.MD5Hash))
                    {
                        beatmapInfos.TryAdd(beatmap.MD5Hash, beatmap);
                    }
                });

                stopwatch.Stop();
                Console.WriteLine($"osu!.db処理時間: {stopwatch.ElapsedMilliseconds:N0}ms");
                
                return beatmapInfos;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"osu!.db読み込みエラー: {ex.Message}");
                throw;
            }
        }


        /// <summary>
        /// 軽量化されたビートマップ情報に変換します
        /// </summary>
        /// <param name="fullBeatmapInfos">フルサイズのビートマップ情報</param>
        /// <returns>軽量化されたビートマップ情報の辞書</returns>
        static ConcurrentDictionary<string, OptimizedBeatmapInfo> ConvertToOptimizedBeatmapInfos(ConcurrentDictionary<string, OsuParsers.Database.Objects.DbBeatmap> fullBeatmapInfos)
        {
            Console.WriteLine("ビートマップ情報を軽量化中...");
            
            var optimizedInfos = new ConcurrentDictionary<string, OptimizedBeatmapInfo>(
                Environment.ProcessorCount,
                fullBeatmapInfos.Count);
            
            Parallel.ForEach(fullBeatmapInfos, new ParallelOptions
            {
                MaxDegreeOfParallelism = Environment.ProcessorCount
            }, kvp =>
            {
                var fullInfo = kvp.Value;
                var optimizedInfo = new OptimizedBeatmapInfo
                {
                    Title = fullInfo.Title,
                    Artist = fullInfo.Artist,
                    Difficulty = fullInfo.Difficulty,
                    Creator = fullInfo.Creator,
                    CircleSize = fullInfo.CircleSize,
                    Ruleset = fullInfo.Ruleset,
                    RankedStatus = fullInfo.RankedStatus,
                    BeatmapSetId = fullInfo.BeatmapSetId,
                    BeatmapId = fullInfo.BeatmapId,
                };
                
                optimizedInfos.TryAdd(kvp.Key, optimizedInfo);
            });
            
            return optimizedInfos;
        }

        /// <summary>
        /// ストリーミング処理でJSONファイルに出力します（メモリ効率化版）
        /// </summary>
        /// <param name="collections">コレクション情報のリスト</param>
        /// <param name="beatmapInfos">ビートマップ詳細情報の辞書</param>
        /// <param name="outputPath">出力ファイルのパス</param>
        static async Task WriteToJsonParallel(List<Collection> collections, ConcurrentDictionary<string, OptimizedBeatmapInfo> beatmapInfos, string outputPath)
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            // ストリーミング処理で直接ファイルに書き込み
            using (var fileStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 65536))
            using (var writer = new Utf8JsonWriter(fileStream, new JsonWriterOptions 
            { 
                Indented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            }))
            {
                writer.WriteStartArray();

                foreach (var collection in collections)
                {
                    writer.WriteStartObject();
                    writer.WriteString("name", collection.Name);
                    writer.WritePropertyName("beatmaps");
                    
                    // ビートマップ配列をストリーミング書き込み
                    writer.WriteStartArray();
                    
                    // 小さなバッチでビートマップを処理してメモリ使用量を制御
                    const int batchSize = 1000;
                    for (int i = 0; i < collection.BeatmapMd5s.Count; i += batchSize)
                    {
                        var batch = collection.BeatmapMd5s.Skip(i).Take(batchSize).ToList();
                        var beatmapBatch = ProcessBeatmapsParallel(batch, beatmapInfos);
                        
                        foreach (var beatmap in beatmapBatch)
                        {
                            JsonSerializer.Serialize(writer, beatmap, options);
                        }
                        
                        // バッチ処理後にガベージコレクションのヒント
                        if (i % 5000 == 0 && i > 0)
                        {
                            GC.Collect(0, GCCollectionMode.Optimized);
                        }
                    }
                    
                    writer.WriteEndArray();
                    writer.WriteEndObject();
                }

                writer.WriteEndArray();
                await writer.FlushAsync();
            }
        }

        /// <summary>
        /// ビートマップリストを並列処理で変換します
        /// </summary>
        /// <param name="beatmapMd5s">MD5ハッシュリスト</param>
        /// <param name="beatmapInfos">ビートマップ詳細情報の辞書</param>
        /// <returns>変換されたビートマップ情報</returns>
        static object[] ProcessBeatmapsParallel(List<string> beatmapMd5s, ConcurrentDictionary<string, OptimizedBeatmapInfo> beatmapInfos)
        {
            return beatmapMd5s.AsParallel()
                .WithDegreeOfParallelism(Environment.ProcessorCount)
                .Select(md5 =>
                {
                    var beatmapInfo = beatmapInfos.TryGetValue(md5, out var info) ? info : null;
                    return new
                    {
                        md5 = md5,
                        title = beatmapInfo?.Title ?? "Unknown",
                        artist = beatmapInfo?.Artist ?? "Unknown",
                        version = beatmapInfo?.Difficulty ?? "Unknown",
                        creator = beatmapInfo?.Creator ?? "Unknown",
                        cs = (double)(beatmapInfo?.CircleSize ?? 0.0f),
                        mode = GetGameModeName(beatmapInfo?.Ruleset ?? OsuParsers.Enums.Ruleset.Standard),
                        status = GetRankedStatusName(beatmapInfo?.RankedStatus),
                        beatmapset_id = beatmapInfo?.BeatmapSetId ?? 0,
                        beatmap_id = beatmapInfo?.BeatmapId ?? 0
                    };
                })
                .ToArray();
        }

        /// <summary>
        /// 指定されたコレクション名でコレクションリストをフィルタリングします
        /// </summary>
        /// <param name="collections">全コレクションのリスト</param>
        /// <param name="targetCollectionName">対象のコレクション名（nullの場合は全コレクション）</param>
        /// <returns>フィルタリングされたコレクションリスト</returns>
        static List<Collection> FilterCollections(List<Collection> collections, string targetCollectionName)
        {
            if (string.IsNullOrEmpty(targetCollectionName))
            {
                // 引数が指定されていない場合は全コレクションを返す
                return collections;
            }
            
            // 完全一致で検索
            var exactMatch = collections.Where(c => c.Name.Equals(targetCollectionName, StringComparison.Ordinal)).ToList();
            if (exactMatch.Count > 0)
            {
                Console.WriteLine($"完全一致でコレクション '{targetCollectionName}' を発見しました。");
                return exactMatch;
            }
            
            // 大文字小文字を無視して検索
            var caseInsensitiveMatch = collections.Where(c => c.Name.Equals(targetCollectionName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (caseInsensitiveMatch.Count > 0)
            {
                Console.WriteLine($"大文字小文字を無視してコレクション '{targetCollectionName}' を発見しました。");
                return caseInsensitiveMatch;
            }
            
            // 部分一致で検索
            var partialMatch = collections.Where(c => c.Name.Contains(targetCollectionName, StringComparison.OrdinalIgnoreCase)).ToList();
            if (partialMatch.Count > 0)
            {
                if (partialMatch.Count == 1)
                {
                    Console.WriteLine($"部分一致でコレクション '{partialMatch[0].Name}' を発見しました。");
                    return partialMatch;
                }
                else
                {
                    Console.WriteLine($"部分一致で複数のコレクションが見つかりました:");
                    foreach (var match in partialMatch)
                    {
                        Console.WriteLine($"  - {match.Name}");
                    }
                    Console.WriteLine("完全なコレクション名を指定してください。");
                    return new List<Collection>();
                }
            }
            
            // 一致するコレクションが見つからない
            return new List<Collection>();
        }

        /// <summary>
        /// ファイル名として無効な文字を安全な文字に置換します
        /// </summary>
        /// <param name="fileName">元のファイル名</param>
        /// <returns>安全なファイル名</returns>
        static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                return "Unknown";
            }

            // Windowsで無効なファイル名文字を置換
            var invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = fileName;
            
            foreach (var invalidChar in invalidChars)
            {
                sanitized = sanitized.Replace(invalidChar, '_');
            }
            
            // その他の問題となる可能性のある文字も置換
            sanitized = sanitized
                .Replace(":", "_")     // コロンは特に問題となりやすい
                .Replace("?", "_")     // クエスチョンマーク
                .Replace("*", "_")     // アスタリスク
                .Replace("\"", "_")    // ダブルクォート
                .Replace("<", "_")     // 小なり
                .Replace(">", "_")     // 大なり
                .Replace("|", "_")     // パイプ
                .Replace("/", "_")     // スラッシュ
                .Replace("\\", "_")    // バックスラッシュ
                .Trim()               // 前後の空白を削除
                .Trim('.');           // 前後のピリオドを削除（Windowsでは問題となる）
            
            // 空文字列や無効な名前になった場合のフォールバック
            if (string.IsNullOrEmpty(sanitized) || sanitized == "." || sanitized == "..")
                return "Unknown";
                
            // 長すぎるファイル名を制限（255文字制限）
            if (sanitized.Length > 200) // .jsonの拡張子を考慮して200文字で制限
                sanitized = sanitized.Substring(0, 200);
                
            return sanitized;
        }

        /// <summary>
        /// OsuParsersのRulesetを文字列に変換します
        /// </summary>
        /// <param name="ruleset">ルールセット</param>
        /// <returns>ゲームモード名</returns>
        static string GetGameModeName(OsuParsers.Enums.Ruleset ruleset)
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
        static string GetRankedStatusName(object? rankedStatus)
        {
            if (rankedStatus == null) return "Unknown";
            
            // enumの値を文字列として取得し、適切な名前にマッピング
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
                _ => statusName // フォールバックとして元の文字列を返す
            };
        }

    }

    /// <summary>
    /// メモリ効率化されたビートマップ情報クラス
    /// </summary>
    public class OptimizedBeatmapInfo
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string Difficulty { get; set; } = string.Empty;
        public string Creator { get; set; } = string.Empty;
        public float CircleSize { get; set; }
        public OsuParsers.Enums.Ruleset Ruleset { get; set; }
        public object? RankedStatus { get; set; }
        public int BeatmapSetId { get; set; }
        public int BeatmapId { get; set; }
    }

    /// <summary>
    /// 設定情報クラス
    /// </summary>
    public class Config
    {
        /// <summary>
        /// osu!フォルダのパス（osu!.exe, collection.db, osu!.db等が配置されているフォルダ）
        /// </summary>
        public string OsuFolderPath { get; set; } = string.Empty;

        /// <summary>
        /// 出力フォルダのパス
        /// </summary>
        public string? OutputFolder { get; set; }

        /// <summary>
        /// 処理対象のコレクション名リスト（単一・複数どちらでも可）
        /// </summary>
        public List<string>? TargetCollections { get; set; }
    }

    /// <summary>
    /// コレクション情報クラス
    /// </summary>
    public class Collection
    {
        /// <summary>
        /// コレクション名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ビートマップのMD5ハッシュリスト
        /// </summary>
        public List<string> BeatmapMd5s { get; set; } = new List<string>();
    }

}