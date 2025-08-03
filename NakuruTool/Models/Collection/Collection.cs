using System.Collections.Generic;

namespace NakuruTool.Models.Collection
{
    /// <summary>
    /// コレクション情報を表現するデータクラス
    /// </summary>
    public class Collection
    {
        /// <summary>
        /// コレクション名
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// ビートマップリスト
        /// </summary>
        public List<Beatmap> Beatmaps { get; set; } = new List<Beatmap>();

        /// <summary>
        /// ビートマップ数
        /// </summary>
        public int BeatmapCount => Beatmaps.Count;
    }
}