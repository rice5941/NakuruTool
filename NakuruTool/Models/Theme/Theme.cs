using System;
using NakuruTool.Models;

namespace NakuruTool.Models.Theme
{
    /// <summary>
    /// テーマ設定を表現するデータクラス
    /// プロパティ変更通知機能を提供し、UIとのデータバインディングをサポートします
    /// </summary>
    public class Theme : NotificationBase
    {
        #region コンストラクタ

        /// <summary>
        /// デフォルトコンストラクタ（ライトテーマで初期化）
        /// </summary>
        public Theme()
        {
            _isDarkTheme = false;
        }

        /// <summary>
        /// 指定されたテーマで初期化するコンストラクタ
        /// </summary>
        /// <param name="isDarkTheme">ダークテーマを使用するかどうか</param>
        public Theme(bool isDarkTheme)
        {
            _isDarkTheme = isDarkTheme;
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// ダークテーマが設定されているかどうか
        /// 値が変更されると、関連するプロパティ（ThemeName、ThemeResourceUri）も自動的に変更通知されます
        /// </summary>
        public bool IsDarkTheme 
        { 
            get { return _isDarkTheme; }
            set { SetProperty(ref _isDarkTheme, value, new string[] { nameof(ThemeName), nameof(ThemeResourceUri) }); }
        }

        /// <summary>
        /// 現在のテーマ名を取得します
        /// </summary>
        public string ThemeName
        {
            get
            {
                return IsDarkTheme ? "ダークテーマ" : "ライトテーマ";
            }
        }

        /// <summary>
        /// テーマリソースファイルのURIを取得します
        /// </summary>
        public string ThemeResourceUri
        {
            get
            {
                return IsDarkTheme 
                    ? "pack://application:,,,/Themes/DarkTheme.xaml"
                    : "pack://application:,,,/Themes/LightTheme.xaml";
            }
        }

        #endregion

        #region 関数

        /// <summary>
        /// テーマを切り替えます
        /// </summary>
        public void SwitchTheme()
        {
            IsDarkTheme = (IsDarkTheme == false);
        }

        /// <summary>
        /// 指定されたテーマに設定します
        /// </summary>
        /// <param name="isDarkTheme">ダークテーマを使用するかどうか</param>
        public void SetTheme(bool isDarkTheme)
        {
            IsDarkTheme = isDarkTheme;
        }

        /// <summary>
        /// このインスタンスのコピーを作成します
        /// </summary>
        /// <returns>テーマ設定のコピー</returns>
        public Theme Clone()
        {
            return new Theme(IsDarkTheme);
        }

        /// <summary>
        /// 指定されたオブジェクトと等しいかどうかを判定します
        /// </summary>
        /// <param name="obj">比較対象のオブジェクト</param>
        /// <returns>等しい場合はtrue</returns>
        public override bool Equals(object obj)
        {
            if (obj is Theme theme)
            {
                return IsDarkTheme == theme.IsDarkTheme;
            }
            return false;
        }

        /// <summary>
        /// ハッシュコードを取得します
        /// </summary>
        /// <returns>ハッシュコード</returns>
        public override int GetHashCode()
        {
            return IsDarkTheme.GetHashCode();
        }

        /// <summary>
        /// 文字列表現を取得します
        /// </summary>
        /// <returns>テーマ名</returns>
        public override string ToString()
        {
            return ThemeName;
        }

        #endregion

        #region メンバ変数

        /// <summary>
        /// ダークテーマが設定されているかどうかのバッキングフィールド
        /// </summary>
        private bool _isDarkTheme;

        #endregion
    }
}