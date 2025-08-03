using Livet;
using NakuruTool.Models.Collection;

namespace NakuruTool.ViewModels.Collection
{
    /// <summary>
    /// View用のビートマップ情報を表現するViewModel
    /// </summary>
    public class BeatmapViewModel : ViewModel
    {
        #region プロパティ

        /// <summary>
        /// タイトル
        /// </summary>
        public string Title
        {
            get { return _title; }
            set { RaisePropertyChangedIfSet(ref _title, value); }
        }

        /// <summary>
        /// アーティスト名
        /// </summary>
        public string Artist
        {
            get { return _artist; }
            set { RaisePropertyChangedIfSet(ref _artist, value); }
        }

        /// <summary>
        /// 難易度名
        /// </summary>
        public string Version
        {
            get { return _version; }
            set { RaisePropertyChangedIfSet(ref _version, value); }
        }

        /// <summary>
        /// 作成者名
        /// </summary>
        public string Creator
        {
            get { return _creator; }
            set { RaisePropertyChangedIfSet(ref _creator, value); }
        }

        /// <summary>
        /// MD5ハッシュ値
        /// </summary>
        public string Md5Hash
        {
            get { return _md5Hash; }
            set { RaisePropertyChangedIfSet(ref _md5Hash, value); }
        }
        
        #endregion

        #region 関数

        /// <summary>
        /// Beatmapデータクラスから変換します
        /// </summary>
        /// <param name="beatmap">変換元のBeatmap</param>
        /// <returns>BeatmapViewModel</returns>
        public static BeatmapViewModel FromBeatmap(Beatmap beatmap)
        {
            return new BeatmapViewModel
            {
                Title = beatmap.Title,
                Artist = beatmap.Artist,
                Version = beatmap.Version,
                Creator = beatmap.Creator,
                Md5Hash = beatmap.Md5Hash
            };
        }

        #endregion
        
        #region メンバ変数
        
        /// <summary>
        /// タイトル
        /// </summary>
        private string _title = string.Empty;
        
        /// <summary>
        /// アーティスト名
        /// </summary>
        private string _artist = string.Empty;
        
        /// <summary>
        /// 難易度名
        /// </summary>
        private string _version = string.Empty;
        
        /// <summary>
        /// 作成者名
        /// </summary>
        private string _creator = string.Empty;
        
        /// <summary>
        /// MD5ハッシュ値
        /// </summary>
        private string _md5Hash = string.Empty;
        
        #endregion
    }
}