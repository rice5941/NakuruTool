using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NakuruTool.Services;

namespace NakuruTool.Models.Collection
{
    /// <summary>
    /// コレクション設定のデータ実体を管理するドメインクラス
    /// </summary>
    public class CollectionDomain
    {
        #region コンストラクタ

        /// <summary>
        /// CollectionDomainクラスの新しいインスタンスを初期化します
        /// </summary>
        public CollectionDomain()
        {
            _collectionStore = new CollectionStore();
        }

        /// <summary>
        /// 指定されたCollectionStoreでCollectionDomainクラスのインスタンスを初期化します
        /// </summary>
        /// <param name="collectionStore">コレクションストア</param>
        public CollectionDomain(CollectionStore collectionStore)
        {
            _collectionStore = collectionStore ?? throw new ArgumentNullException(nameof(collectionStore));
        }

        #endregion

        #region 関数

        /// <summary>
        /// 指定されたosu!フォルダからコレクションを読み込みます
        /// </summary>
        /// <param name="osuFolderPath">osu!フォルダのパス</param>
        /// <returns>コレクションのリスト</returns>
        public async Task<List<Collection>> LoadCollectionsAsync(string osuFolderPath)
        {
            if (string.IsNullOrEmpty(osuFolderPath))
            {
                var message = LanguageManager.GetString("OsuFolderPathNotSpecified") ?? "osu!フォルダパスが指定されていません";
                throw new ArgumentException(message, nameof(osuFolderPath));
            }

            return await _collectionStore.LoadCollectionsAsync(osuFolderPath);
        }

        /// <summary>
        /// コレクションをファイルに保存します
        /// </summary>
        /// <param name="collections">保存するコレクションのリスト</param>
        /// <param name="filePath">保存先ファイルパス</param>
        public async Task SaveCollectionsAsync(List<Collection> collections, string filePath)
        {
            if (collections == null)
            {
                throw new ArgumentNullException(nameof(collections));
            }

            if (string.IsNullOrEmpty(filePath))
            {
                var message = LanguageManager.GetString("FilePathNotSpecified") ?? "ファイルパスが指定されていません";
                throw new ArgumentException(message, nameof(filePath));
            }

            await _collectionStore.SaveCollectionsAsync(collections, filePath);
        }

        #endregion

        #region メンバ変数

        /// <summary>
        /// コレクションストア
        /// </summary>
        private readonly CollectionStore _collectionStore;

        #endregion
    }
}