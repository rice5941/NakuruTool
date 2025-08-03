namespace NakuruTool.Models.Collection
{
    /// <summary>
    /// ビートマップ情報を表現するデータクラス
    /// </summary>
    public class Beatmap
    {
        /// <summary>
        /// タイトル
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// アーティスト名
        /// </summary>
        public string Artist { get; set; } = string.Empty;

        /// <summary>
        /// 難易度名
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// 作成者名
        /// </summary>
        public string Creator { get; set; } = string.Empty;

        /// <summary>
        /// MD5ハッシュ値
        /// </summary>
        public string Md5Hash { get; set; } = string.Empty;
    }
}