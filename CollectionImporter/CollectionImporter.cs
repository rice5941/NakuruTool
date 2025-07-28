using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using OsuParsers.Database;
using OsuParsers.Database.Objects;
using OsuParsers.Decoders;

namespace CollectionImporter
{
    /// <summary>
    /// JSON形式のコレクションファイルを読み込み、collection.dbファイルに上書きするコンソールアプリケーション
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;
            
            Console.WriteLine("=== osu! Collection Importer ===");
            Console.WriteLine();
            
            // 引数処理
            string? customBackupPath = null;
            if (args.Length > 0)
            {
                // 引数がある場合は最初の引数をバックアップ先として使用
                customBackupPath = args[0];
                Console.WriteLine($"Custom backup path: {customBackupPath}");
            }
            
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
            
            // 入力フォルダとファイルパスの設定
            string inputFolder = config.InputFolder ?? "input";
            string osuFolder = config.OsuFolderPath;
            string collectionDbPath = Path.Combine(osuFolder, "collection.db");
            string osuDbPath = Path.Combine(osuFolder, "osu!.db");
            
            // バックアップフォルダの決定と作成
            string backupFolder;
            if (!string.IsNullOrEmpty(customBackupPath))
            {
                // 引数でバックアップ先が指定された場合
                backupFolder = customBackupPath;
            }
            else
            {
                // デフォルト: 実行ファイルと同階層のbackupフォルダ
                string? exeDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                if (exeDirectory == null)
                {
                    throw new InvalidOperationException("実行ファイルのディレクトリを取得できませんでした。");
                }
                backupFolder = Path.Combine(exeDirectory, "backup");
            }
            
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
                Console.WriteLine($"Created backup folder: {backupFolder}");
            }
            
            try
            {
                // 既存のcollection.dbのバックアップ
                if (File.Exists(collectionDbPath))
                {
                    string backupPath = Path.Combine(backupFolder, $"collection_backup_{DateTime.Now:yyyyMMdd_HHmmss}.db");
                    File.Copy(collectionDbPath, backupPath);
                    Console.WriteLine($"Backup created: {backupPath}");
                }
                
                // osu!.dbを読み込んでMD5ハッシュの検証用辞書を作成
                Console.WriteLine("Loading osu!.db for MD5 hash validation...");
                var osuDb = DatabaseDecoder.DecodeOsu(osuDbPath);
                var validMd5Hashes = new HashSet<string>(osuDb.Beatmaps.Select(b => b.MD5Hash).Where(h => !string.IsNullOrEmpty(h)));
                Console.WriteLine($"Loaded {validMd5Hashes.Count} valid MD5 hashes");
                
                // 既存のcollection.dbからコレクションを読み込み
                var existingCollections = new List<Collection>();
                if (File.Exists(collectionDbPath))
                {
                    try
                    {
                        var existingDb = DatabaseDecoder.DecodeCollection(collectionDbPath);
                        // OsuParsers.Database.Objects.CollectionからプロジェクトのCollectionに変換
                        foreach (var osuCollection in existingDb.Collections)
                        {
                            var collection = new Collection
                            {
                                Name = osuCollection.Name,
                                MD5Hashes = osuCollection.MD5Hashes.ToList()
                            };
                            existingCollections.Add(collection);
                        }
                        Console.WriteLine($"Loaded {existingCollections.Count} existing collections from collection.db");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Warning: Could not read existing collection.db: {ex.Message}");
                    }
                }
                
                // JSONファイルからコレクションを読み込み
                var newCollections = await LoadCollectionsFromJson(inputFolder, validMd5Hashes);
                
                if (!newCollections.Any())
                {
                    Console.WriteLine("No valid collections found in JSON files.");
                    return;
                }
                
                Console.WriteLine($"Loaded {newCollections.Count} collections from JSON files");
                
                // 既存のコレクションと新しいコレクションを結合
                var allCollections = new List<Collection>(existingCollections);
                foreach (var newCollection in newCollections)
                {
                    // 同名のコレクションがある場合は上書き
                    var existingIndex = allCollections.FindIndex(c => c.Name == newCollection.Name);
                    if (existingIndex >= 0)
                    {
                        allCollections[existingIndex] = newCollection;
                        Console.WriteLine($"Updated existing collection: {newCollection.Name}");
                    }
                    else
                    {
                        allCollections.Add(newCollection);
                        Console.WriteLine($"Added new collection: {newCollection.Name}");
                    }
                }
                
                // collection.dbを作成
                await WriteCollectionDb(allCollections, collectionDbPath);
                
                Console.WriteLine($"\nImport completed! Collection database updated: {collectionDbPath}");
                Console.WriteLine($"Total collections in database: {allCollections.Count}");
                Console.WriteLine($"Imported from JSON: {newCollections.Count} collections");
                
                foreach (var collection in newCollections)
                {
                    Console.WriteLine($"  - {collection.Name}: {collection.MD5Hashes.Count} beatmaps");
                }
                
                // 引数なしの場合のみEnterキー待機
                if (args.Length == 0)
                {
                    Console.WriteLine("\nPress Enter to exit...");
                    Console.ReadLine();
                }
                
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
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
            Console.WriteLine($"│ Input Folder    : {config.InputFolder ?? "input",-42} │");
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
                // デフォルト設定を返す（ファイル作成はしない）
                return new Config
                {
                    OsuFolderPath = "C:/Users/YourUsername/AppData/Local/osu!",
                    InputFolder = "input"
                };
            }
            
            var configJson = File.ReadAllText(configPath, Encoding.UTF8);
            return JsonSerializer.Deserialize<Config>(configJson) ?? new Config
            {
                OsuFolderPath = "C:/Users/YourUsername/AppData/Local/osu!",
                InputFolder = "input"
            };
        }

        /// <summary>
        /// JSONファイルからコレクションを読み込みます
        /// </summary>
        /// <param name="inputFolder">入力フォルダのパス</param>
        /// <param name="validMd5Hashes">有効なMD5ハッシュのセット</param>
        /// <returns>読み込まれたコレクション</returns>
        static async Task<List<Collection>> LoadCollectionsFromJson(string inputFolder, HashSet<string> validMd5Hashes)
        {
            var collections = new List<Collection>();
            
            if (!Directory.Exists(inputFolder))
            {
                Console.WriteLine($"Input folder not found: {inputFolder}");
                return collections;
            }
            
            var jsonFiles = Directory.GetFiles(inputFolder, "*.json");
            if (!jsonFiles.Any())
            {
                Console.WriteLine($"No JSON files found in: {inputFolder}");
                return collections;
            }
            
            Console.WriteLine($"Processing {jsonFiles.Length} JSON files...");
            
            foreach (var jsonFile in jsonFiles)
            {
                try
                {
                    var jsonContent = await File.ReadAllTextAsync(jsonFile, Encoding.UTF8);
                    var jsonData = JsonSerializer.Deserialize<JsonElement>(jsonContent);
                    
                    // 単一コレクションまたは複数コレクションの配列を処理
                    if (jsonData.ValueKind == JsonValueKind.Array)
                    {
                        foreach (var item in jsonData.EnumerateArray())
                        {
                            var collection = ParseCollectionFromJson(item, validMd5Hashes);
                            if (collection != null)
                            {
                                collections.Add(collection);
                            }
                        }
                    }
                    else if (jsonData.ValueKind == JsonValueKind.Object)
                    {
                        var collection = ParseCollectionFromJson(jsonData, validMd5Hashes);
                        if (collection != null)
                        {
                            collections.Add(collection);
                        }
                    }
                    
                    Console.WriteLine($"Processed: {Path.GetFileName(jsonFile)}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error processing {Path.GetFileName(jsonFile)}: {ex.Message}");
                }
            }
            
            return collections;
        }

        /// <summary>
        /// JSON要素からコレクションを解析します
        /// </summary>
        /// <param name="jsonElement">JSON要素</param>
        /// <param name="validMd5Hashes">有効なMD5ハッシュのセット</param>
        /// <returns>解析されたコレクション</returns>
        static Collection? ParseCollectionFromJson(JsonElement jsonElement, HashSet<string> validMd5Hashes)
        {
            if (!jsonElement.TryGetProperty("name", out var nameElement) ||
                !jsonElement.TryGetProperty("beatmaps", out var beatmapsElement))
            {
                return null;
            }
            
            var collection = new Collection
            {
                Name = nameElement.GetString() ?? "Unknown Collection"
            };
            
            var md5Hashes = new List<string>();
            
            if (beatmapsElement.ValueKind == JsonValueKind.Array)
            {
                foreach (var beatmap in beatmapsElement.EnumerateArray())
                {
                    if (beatmap.TryGetProperty("md5", out var md5Element))
                    {
                        var md5Hash = md5Element.GetString();
                        if (!string.IsNullOrEmpty(md5Hash) && validMd5Hashes.Contains(md5Hash))
                        {
                            md5Hashes.Add(md5Hash);
                        }
                    }
                }
            }
            
            collection.MD5Hashes = md5Hashes;
            
            Console.WriteLine($"  Collection '{collection.Name}': {md5Hashes.Count} valid beatmaps");
            
            return collection;
        }

        /// <summary>
        /// collection.dbファイルを書き込みます
        /// </summary>
        /// <param name="collections">書き込むコレクション</param>
        /// <param name="collectionDbPath">collection.dbファイルのパス</param>
        static async Task WriteCollectionDb(List<Collection> collections, string collectionDbPath)
        {
            Console.WriteLine("Writing collection.db...");
            
            using var fileStream = new FileStream(collectionDbPath, FileMode.Create, FileAccess.Write);
            using var writer = new BinaryWriter(fileStream, Encoding.UTF8);
            
            // バージョン情報を書き込み（20210528）
            writer.Write(20210528);
            
            // コレクション数を書き込み
            writer.Write(collections.Count);
            
            // 各コレクションを書き込み
            foreach (var collection in collections)
            {
                // コレクション名を書き込み
                WriteOsuString(writer, collection.Name);
                
                // ビートマップ数を書き込み
                writer.Write(collection.MD5Hashes.Count);
                
                // MD5ハッシュリストを書き込み
                foreach (var md5Hash in collection.MD5Hashes)
                {
                    WriteOsuString(writer, md5Hash);
                }
            }
            
            await fileStream.FlushAsync();
            Console.WriteLine($"Successfully wrote {collections.Count} collections to collection.db");
        }

        /// <summary>
        /// osu!形式の文字列を書き込みます
        /// プレフィックスバイト + ULEB128長さ + UTF-8文字列
        /// </summary>
        /// <param name="writer">バイナリライター</param>
        /// <param name="value">書き込む文字列</param>
        static void WriteOsuString(BinaryWriter writer, string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // 空文字列
                writer.Write((byte)0x00);
            }
            else
            {
                // 文字列あり
                writer.Write((byte)0x0b);
                var bytes = Encoding.UTF8.GetBytes(value);
                WriteULEB128(writer, (uint)bytes.Length);
                writer.Write(bytes);
            }
        }

        /// <summary>
        /// ULEB128形式の整数を書き込みます
        /// </summary>
        /// <param name="writer">バイナリライター</param>
        /// <param name="value">書き込む整数値</param>
        static void WriteULEB128(BinaryWriter writer, uint value)
        {
            while (value >= 0x80)
            {
                writer.Write((byte)(value | 0x80));
                value >>= 7;
            }
            writer.Write((byte)value);
        }
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
        /// 入力フォルダのパス（JSONファイルが配置されているフォルダ）
        /// </summary>
        public string? InputFolder { get; set; }
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
        public List<string> MD5Hashes { get; set; } = new List<string>();
    }
}