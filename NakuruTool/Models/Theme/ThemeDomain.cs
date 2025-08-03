using System;
using System.Windows;

namespace NakuruTool.Models.Theme
{
    /// <summary>
    /// テーマ設定のデータ実体を管理するドメインクラス
    /// </summary>
    public class ThemeDomain
    {
        #region コンストラクタ

        /// <summary>
        /// ThemeDomainクラスの新しいインスタンスを初期化します
        /// </summary>
        public ThemeDomain()
        {
            _themeStore = new ThemeStore();
            _currentTheme = _themeStore.LoadTheme();
        }

        /// <summary>
        /// 指定されたThemeStoreでThemeDomainクラスのインスタンスを初期化します
        /// </summary>
        /// <param name="themeStore">テーマストア</param>
        public ThemeDomain(ThemeStore themeStore)
        {
            _themeStore = themeStore ?? throw new ArgumentNullException(nameof(themeStore));
            _currentTheme = _themeStore.LoadTheme();
        }

        #endregion

        #region プロパティ

        /// <summary>
        /// 現在のテーマ設定
        /// </summary>
        public Theme CurrentTheme
        {
            get { return _currentTheme; }
            private set
            {
                if (_currentTheme?.Equals(value) == false || _currentTheme == null)
                {
                    _currentTheme = value;
                    OnThemeChanged?.Invoke(_currentTheme);
                }
            }
        }

        /// <summary>
        /// 現在のテーマがダークテーマかどうか
        /// </summary>
        public bool IsDarkTheme
        {
            get { return _currentTheme?.IsDarkTheme ?? false; }
        }

        /// <summary>
        /// 現在のテーマ名
        /// </summary>
        public string ThemeName
        {
            get { return _currentTheme?.ThemeName ?? "ライトテーマ"; }
        }

        /// <summary>
        /// テーマリソースURI
        /// </summary>
        public string ThemeResourceUri
        {
            get { return _currentTheme?.ThemeResourceUri ?? "pack://application:,,,/Themes/LightTheme.xaml"; }
        }

        #endregion

        #region 関数

        /// <summary>
        /// テーマを初期化します
        /// </summary>
        public void Initialize()
        {
            CurrentTheme = _themeStore.LoadTheme();
        }

        /// <summary>
        /// テーマを切り替えます
        /// </summary>
        /// <returns>切り替えに成功した場合はtrue</returns>
        public bool SwitchTheme()
        {
            try
            {
                var newTheme = CurrentTheme.Clone();
                newTheme.SwitchTheme();
                
                if (_themeStore.SaveTheme(newTheme))
                {
                    CurrentTheme = newTheme;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to switch theme: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// 指定されたテーマタイプに設定します
        /// </summary>
        /// <param name="themeType">テーマタイプ</param>
        /// <returns>設定に成功した場合はtrue</returns>
        public bool SetTheme(ThemeType themeType)
        {
            try
            {
                var newTheme = new Theme(themeType);
                
                if (_themeStore.SaveTheme(newTheme))
                {
                    CurrentTheme = newTheme;
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to set theme: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// テーマをデフォルトにリセットします
        /// </summary>
        /// <returns>リセットに成功した場合はtrue</returns>
        public bool ResetTheme()
        {
            try
            {
                if (_themeStore.ResetTheme())
                {
                    CurrentTheme = _themeStore.CreateDefaultTheme();
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reset theme: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// 現在のテーマをWPFアプリケーションに適用します
        /// </summary>
        /// <returns>適用に成功した場合はtrue</returns>
        public bool ApplyThemeToApplication()
        {
            try
            {
                var app = Application.Current;
                if (app == null)
                {
                    return false;
                }

                // 既存のテーマリソースを削除
                var mergedDictionaries = app.Resources.MergedDictionaries;
                
                for (int i = mergedDictionaries.Count - 1; i >= 0; i--)
                {
                    var dict = mergedDictionaries[i];
                    if (dict.Source != null && 
                        (dict.Source.ToString().Contains("LightTheme.xaml") || 
                         dict.Source.ToString().Contains("DarkTheme.xaml") ||
                         dict.Source.ToString().Contains("OsuTheme.xaml")))
                    {
                        mergedDictionaries.RemoveAt(i);
                    }
                }

                // 新しいテーマを読み込み
                var themeUri = new Uri(CurrentTheme.ThemeResourceUri);
                var themeResource = new ResourceDictionary { Source = themeUri };
                mergedDictionaries.Add(themeResource);

                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to apply theme to application: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// テーマ設定を再読み込みします
        /// </summary>
        /// <returns>再読み込みに成功した場合はtrue</returns>
        public bool ReloadTheme()
        {
            try
            {
                CurrentTheme = _themeStore.LoadTheme();
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to reload theme: {ex.Message}");
                return false;
            }
        }

        #endregion

        #region イベント

        /// <summary>
        /// テーマが変更された際に発生するイベント
        /// </summary>
        public event Action<Theme> OnThemeChanged;

        #endregion

        #region メンバ変数

        /// <summary>
        /// テーマストア
        /// </summary>
        private readonly ThemeStore _themeStore;

        /// <summary>
        /// 現在のテーマ設定
        /// </summary>
        private Theme _currentTheme;

        #endregion
    }
}