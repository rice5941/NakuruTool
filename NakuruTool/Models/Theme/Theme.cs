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
            _themeType = ThemeType.Light;
        }

        /// <summary>
        /// 指定されたテーマで初期化するコンストラクタ
        /// </summary>
        /// <param name="isDarkTheme">ダークテーマを使用するかどうか</param>
        public Theme(bool isDarkTheme)
        {
            _themeType = isDarkTheme ? ThemeType.Dark : ThemeType.Light;
        }

        /// <summary>
        /// 指定されたテーマタイプで初期化するコンストラクタ
        /// </summary>
        /// <param name="themeType">テーマタイプ</param>
        public Theme(ThemeType themeType)
        {
            _themeType = themeType;
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// 現在のテーマタイプ
        /// 値が変更されると、関連するプロパティ（ThemeName、ThemeResourceUri、IsDarkTheme）も自動的に変更通知されます
        /// </summary>
        public ThemeType CurrentThemeType
        {
            get { return _themeType; }
            set { SetProperty(ref _themeType, value, new string[] { nameof(ThemeName), nameof(ThemeResourceUri), nameof(IsDarkTheme) }); }
        }

        /// <summary>
        /// ダークテーマが設定されているかどうか
        /// </summary>
        public bool IsDarkTheme 
        { 
            get { return _themeType == ThemeType.Dark; }
            set 
            { 
                CurrentThemeType = value ? ThemeType.Dark : ThemeType.Light;
            }
        }

        /// <summary>
        /// 現在のテーマ名を取得します
        /// </summary>
        public string ThemeName
        {
            get
            {
                switch (_themeType)
                {
                    case ThemeType.Dark:
                        return "ダークテーマ";
                    case ThemeType.Osu:
                        return "osu!テーマ";
                    case ThemeType.Light:
                    default:
                        return "ライトテーマ";
                }
            }
        }

        /// <summary>
        /// テーマリソースファイルのURIを取得します
        /// </summary>
        public string ThemeResourceUri
        {
            get
            {
                switch (_themeType)
                {
                    case ThemeType.Dark:
                        return "pack://application:,,,/Themes/DarkTheme.xaml";
                    case ThemeType.Osu:
                        return "pack://application:,,,/Themes/OsuTheme.xaml";
                    case ThemeType.Light:
                    default:
                        return "pack://application:,,,/Themes/LightTheme.xaml";
                }
            }
        }

        #endregion

        #region 関数

        /// <summary>
        /// テーマを切り替えます（次のテーマへ循環）
        /// </summary>
        public void SwitchTheme()
        {
            switch (_themeType)
            {
                case ThemeType.Light:
                    CurrentThemeType = ThemeType.Dark;
                    break;
                case ThemeType.Dark:
                    CurrentThemeType = ThemeType.Osu;
                    break;
                case ThemeType.Osu:
                    CurrentThemeType = ThemeType.Light;
                    break;
            }
        }


        /// <summary>
        /// 指定されたテーマタイプに設定します
        /// </summary>
        /// <param name="themeType">テーマタイプ</param>
        public void SetTheme(ThemeType themeType)
        {
            CurrentThemeType = themeType;
        }

        /// <summary>
        /// このインスタンスのコピーを作成します
        /// </summary>
        /// <returns>テーマ設定のコピー</returns>
        public Theme Clone()
        {
            return new Theme(_themeType);
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
                return _themeType == theme._themeType;
            }
            return false;
        }

        /// <summary>
        /// ハッシュコードを取得します
        /// </summary>
        /// <returns>ハッシュコード</returns>
        public override int GetHashCode()
        {
            return _themeType.GetHashCode();
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
        /// 現在のテーマタイプのバッキングフィールド
        /// </summary>
        private ThemeType _themeType;

        #endregion
    }
}