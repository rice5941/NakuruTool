using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OsuParsers.Database;
using OsuParsers.Decoders;
using NakuruTool.Services;

namespace NakuruTool.Models.Collection
{
    /// <summary>
    /// コレクションのデータ操作を行うクラス
    /// </summary>
    public class CollectionStore
    {
        #region 関数

        /// <summary>
        /// 指定されたosu!フォルダからコレクションを非同期で読み込みます
        /// </summary>
        /// <param name="osuFolderPath">osu!フォルダのパス</param>
        /// <returns>コレクションのリスト</returns>
        public async Task<List<Collection>> LoadCollectionsAsync(string osuFolderPath)
        {
            var collectionDbPath = Path.Combine(osuFolderPath, "collection.db");
            var osuDbPath = Path.Combine(osuFolderPath, "osu!.db");

            ValidateFiles(collectionDbPath, osuDbPath);

            var collectionTask = Task.Run(() => ReadCollectionDb(collectionDbPath));
            var osuDbTask = Task.Run(() => ReadOsuDb(osuDbPath));

            var rawCollections = await collectionTask;
            var beatmapInfos = await osuDbTask;

            return await ConvertToCollections(rawCollections, beatmapInfos);
        }

        /// <summary>
        /// コレクションをファイルに保存します
        /// </summary>
        /// <param name="collections">保存するコレクションのリスト</param>
        /// <param name="filePath">保存先ファイルパス</param>
        public async Task SaveCollectionsAsync(List<Collection> collections, string filePath)
        {
            await Task.Run(() =>
            {
                // TODO: ファイル保存機能の実装（将来の拡張用）
                var message = LanguageManager.GetString("CollectionSaveNotImplemented") ?? "コレクション保存機能は未実装です";
                throw new NotImplementedException(message);
            });
        }

        /// <summary>
        /// ファイルの存在を検証します
        /// </summary>
        /// <param name="collectionDbPath">collection.dbファイルのパス</param>
        /// <param name="osuDbPath">osu!.dbファイルのパス</param>
        private void ValidateFiles(string collectionDbPath, string osuDbPath)
        {
            if (File.Exists(collectionDbPath) == false)
            {
                var messageTemplate = LanguageManager.GetString("CollectionDbNotFound") ?? "collection.dbファイルが見つかりません: {0}";
                var message = string.Format(messageTemplate, collectionDbPath);
                throw new FileNotFoundException(message);
            }

            if (File.Exists(osuDbPath) == false)
            {
                var messageTemplate = LanguageManager.GetString("OsuDbNotFound") ?? "osu!.dbファイルが見つかりません: {0}";
                var message = string.Format(messageTemplate, osuDbPath);
                throw new FileNotFoundException(message);
            }
        }

        /// <summary>
        /// collection.dbファイルを読み込みます
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>Raw コレクションのリスト</returns>
        private List<RawCollection> ReadCollectionDb(string filePath)
        {
            var collections = new List<RawCollection>();

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(fileStream, Encoding.UTF8))
            {
                var version = reader.ReadInt32();
                var collectionCount = reader.ReadInt32();

                for (int i = 0; i < collectionCount; i++)
                {
                    var collection = new RawCollection();
                    collection.Name = ReadOsuString(reader);

                    var beatmapCount = reader.ReadInt32();
                    collection.BeatmapMd5s = new List<string>();

                    for (int j = 0; j < beatmapCount; j++)
                    {
                        var md5Hash = ReadOsuString(reader);
                        collection.BeatmapMd5s.Add(md5Hash);
                    }

                    collections.Add(collection);
                }
            }

            return collections;
        }

        /// <summary>
        /// osu!形式の文字列を読み込みます
        /// </summary>
        /// <param name="reader">バイナリリーダー</param>
        /// <returns>読み込んだ文字列</returns>
        private string ReadOsuString(BinaryReader reader)
        {
            var prefix = reader.ReadByte();

            if (prefix == 0x00)
            {
                return string.Empty;
            }
            else if (prefix == 0x0b)
            {
                var length = ReadULEB128(reader);
                var bytes = reader.ReadBytes((int)length);
                return Encoding.UTF8.GetString(bytes);
            }
            else
            {
                var messageTemplate = LanguageManager.GetString("InvalidStringPrefix") ?? "無効な文字列プレフィックス: 0x{0:X2}";
                var message = string.Format(messageTemplate, prefix);
                throw new InvalidDataException(message);
            }
        }

        /// <summary>
        /// ULEB128形式の整数を読み込みます
        /// </summary>
        /// <param name="reader">バイナリリーダー</param>
        /// <returns>読み込んだ整数値</returns>
        private uint ReadULEB128(BinaryReader reader)
        {
            uint result = 0;
            int shift = 0;

            while (true)
            {
                var b = reader.ReadByte();
                result |= (uint)(b & 0x7F) << shift;

                if ((b & 0x80) == 0)
                {
                    break;
                }

                shift += 7;
            }

            return result;
        }

        /// <summary>
        /// osu!.dbファイルを読み込みます
        /// </summary>
        /// <param name="filePath">ファイルパス</param>
        /// <returns>MD5ハッシュをキーとしたビートマップ情報の辞書</returns>
        private ConcurrentDictionary<string, OsuParsers.Database.Objects.DbBeatmap> ReadOsuDb(string filePath)
        {
            var osuDatabase = DatabaseDecoder.DecodeOsu(filePath);
            var beatmapInfos = new ConcurrentDictionary<string, OsuParsers.Database.Objects.DbBeatmap>();

            Parallel.ForEach(osuDatabase.Beatmaps, beatmap =>
            {
                if (string.IsNullOrEmpty(beatmap.MD5Hash) == false)
                {
                    beatmapInfos.TryAdd(beatmap.MD5Hash, beatmap);
                }
            });

            return beatmapInfos;
        }

        /// <summary>
        /// Raw コレクションをコレクションに変換します
        /// </summary>
        /// <param name="rawCollections">Raw コレクションのリスト</param>
        /// <param name="beatmapInfos">ビートマップ情報の辞書</param>
        /// <returns>コレクションのリスト</returns>
        private async Task<List<Collection>> ConvertToCollections(
            List<RawCollection> rawCollections,
            ConcurrentDictionary<string, OsuParsers.Database.Objects.DbBeatmap> beatmapInfos)
        {
            return await Task.Run(() =>
            {
                var collections = new List<Collection>();

                foreach (var rawCollection in rawCollections)
                {
                    var collection = new Collection
                    {
                        Name = rawCollection.Name
                    };

                    var beatmaps = new List<Beatmap>();

                    foreach (var md5Hash in rawCollection.BeatmapMd5s)
                    {
                        var beatmap = new Beatmap
                        {
                            Md5Hash = md5Hash
                        };

                        var unknownText = LanguageManager.GetString("Unknown") ?? "Unknown";
                        
                        if (beatmapInfos.TryGetValue(md5Hash, out var beatmapInfo))
                        {
                            beatmap.Title = beatmapInfo.Title ?? unknownText;
                            beatmap.Artist = beatmapInfo.Artist ?? unknownText;
                            beatmap.Version = beatmapInfo.Difficulty ?? unknownText;
                            beatmap.Creator = beatmapInfo.Creator ?? unknownText;
                        }
                        else
                        {
                            beatmap.Title = unknownText;
                            beatmap.Artist = unknownText;
                            beatmap.Version = unknownText;
                            beatmap.Creator = unknownText;
                        }

                        beatmaps.Add(beatmap);
                    }

                    collection.Beatmaps = beatmaps;
                    collections.Add(collection);
                }

                return collections;
            });
        }

        #endregion

        #region 内部クラス

        /// <summary>
        /// Raw コレクション情報を格納するクラス
        /// </summary>
        private class RawCollection
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

        #endregion
    }
}