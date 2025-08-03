using System;
using NakuruTool.Services;

namespace NakuruTool.Models.Theme
{
    /// <summary>
    /// テーマ設定のデータ操作を行うクラス
    /// </summary>
    public class ThemeStore
    {
        #region コンストラクタ

        /// <summary>
        /// ThemeStoreクラスの新しいインスタンスを初期化します
        /// </summary>
        public ThemeStore()
        {
        }

        #endregion

        #region 関数

        /// <summary>
        /// 現在のテーマ設定を読み込みます
        /// </summary>
        /// <returns>現在のテーマ設定</returns>
        public Theme LoadTheme()
        {
            try
            {
                // ConfigManagerから設定を読み込み
                ConfigManager.LoadConfig();
                var themeTypeValue = ConfigManager.CurrentConfig.ThemeType;
                
                // 文字列からThemeTypeに変換
                if (Enum.TryParse<ThemeType>(themeTypeValue, out var themeType))
                {
                    return new Theme(themeType);
                }
                
                // 互換性のため、古いIsDarkTheme設定もチェック
                var isDarkTheme = ConfigManager.CurrentConfig.IsDarkTheme;
                return new Theme(isDarkTheme ? ThemeType.Dark : ThemeType.Light);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load theme: {ex.Message}");
                // フォールバック：デフォルトのライトテーマを返す
                return new Theme(ThemeType.Light);
            }
        }

        /// <summary>
        /// テーマ設定を保存します
        /// </summary>
        /// <param name="theme">保存するテーマ設定</param>
        /// <returns>保存に成功した場合はtrue</returns>
        public bool SaveTheme(Theme theme)
        {
            if (theme == null)
            {
                throw new ArgumentNullException(nameof(theme));
            }

            try
            {
                // ThemeTypeを文字列として保存
                ConfigManager.UpdateThemeType(theme.CurrentThemeType.ToString());
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save theme: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// デフォルトのテーマ設定を作成します
        /// </summary>
        /// <returns>デフォルトテーマ（ライトテーマ）</returns>
        public Theme CreateDefaultTheme()
        {
            return new Theme(ThemeType.Light);
        }

        /// <summary>
        /// テーマ設定をリセットしてデフォルトに戻します
        /// </summary>
        /// <returns>リセットに成功した場合はtrue</returns>
        public bool ResetTheme()
        {
            try
            {
                var defaultTheme = CreateDefaultTheme();
                return SaveTheme(defaultTheme);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reset theme: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// テーマが存在するかどうかを確認します
        /// </summary>
        /// <returns>テーマ設定が存在する場合はtrue</returns>
        public bool ThemeExists()
        {
            try
            {
                var theme = LoadTheme();
                return theme != null;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 現在のテーマ設定のバックアップを作成します
        /// </summary>
        /// <returns>バックアップされたテーマ設定</returns>
        public Theme BackupCurrentTheme()
        {
            var currentTheme = LoadTheme();
            return currentTheme?.Clone();
        }

        /// <summary>
        /// バックアップからテーマ設定を復元します
        /// </summary>
        /// <param name="backupTheme">復元するテーマ設定</param>
        /// <returns>復元に成功した場合はtrue</returns>
        public bool RestoreTheme(Theme backupTheme)
        {
            if (backupTheme == null)
            {
                throw new ArgumentNullException(nameof(backupTheme));
            }

            return SaveTheme(backupTheme);
        }

        #endregion
    }
}