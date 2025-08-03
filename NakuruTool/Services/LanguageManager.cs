using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml.Linq;

namespace NakuruTool.Services
{
    /// <summary>
    /// アプリケーションの多言語対応を管理するクラス
    /// </summary>
    public static class LanguageManager
    {
        #region プロパティ
        
        /// <summary>
        /// 現在の言語設定
        /// </summary>
        public static string CurrentLanguage { get; private set; } = "ja-JP";
        
        #endregion
        
        #region メンバ変数
        
        /// <summary>
        /// 利用可能な言語リストと対応するリソースキー
        /// </summary>
        private static readonly Dictionary<string, string> AvailableLanguages = new Dictionary<string, string>
        {
            { "ja-JP", "Language_ja" },
            { "en-US", "Language_en" },
            { "zh-CN", "Language_zh" },
            { "ko-KR", "Language_ko" },
            { "tl-PH", "Language_tl" },
            { "th-TH", "Language_th" },
            { "vi-VN", "Language_vi" },
            { "de-DE", "Language_de" },
            { "fr-FR", "Language_fr" },
            { "es-ES", "Language_es" }
        };
        
        #endregion

        #region 関数
        
        /// <summary>
        /// 利用可能な言語一覧を取得します
        /// </summary>
        /// <returns>言語コードとリソースキーのペア</returns>
        public static IEnumerable<KeyValuePair<string, string>> GetAvailableLanguages()
        {
            return AvailableLanguages.AsEnumerable();
        }

        /// <summary>
        /// 言語マネージャーを初期化します
        /// </summary>
        /// <param name="savedLanguage">保存された言語設定</param>
        public static void Initialize(string savedLanguage = null)
        {
            if (!string.IsNullOrEmpty(savedLanguage) && AvailableLanguages.ContainsKey(savedLanguage))
            {
                CurrentLanguage = savedLanguage;
            }
            else
            {
                // 保存された言語設定を読み込みまたはシステム言語を取得
                LoadLanguageSettings();
            }
            
            // 初期言語リソースを設定
            LoadLanguageResource(CurrentLanguage);
        }

        /// <summary>
        /// 言語を変更します
        /// </summary>
        /// <param name="languageCode">変更先の言語コード</param>
        public static void ChangeLanguage(string languageCode)
        {
            if (!AvailableLanguages.ContainsKey(languageCode))
            {
                throw new ArgumentException($"Language {languageCode} is not supported");
            }

            CurrentLanguage = languageCode;
            LoadLanguageResource(languageCode);
            
            // ConfigManagerに言語設定を保存（統一設定システム）
            ConfigManager.UpdateLanguage(languageCode);
        }

        private static void LoadLanguageResource(string languageCode)
        {
            var app = Application.Current;
            if (app == null) return;

            // 既存の言語リソースを削除
            var mergedDictionaries = app.Resources.MergedDictionaries;
            for (int i = mergedDictionaries.Count - 1; i >= 0; i--)
            {
                var dict = mergedDictionaries[i];
                if (dict.Source != null && dict.Source.ToString().Contains("Languages/"))
                {
                    mergedDictionaries.RemoveAt(i);
                }
            }

            // XMLファイルから言語データを読み込み、ResourceDictionaryを作成
            try
            {
                LoadLanguageFromXml(languageCode);
            }
            catch (Exception ex)
            {
                // フォールバックとして日本語を読み込み
                if (languageCode != "ja-JP")
                {
                    LoadLanguageResource("ja-JP");
                }
                System.Diagnostics.Debug.WriteLine($"Failed to load language resource {languageCode}: {ex.Message}");
            }
        }

        /// <summary>
        /// 保存された言語設定をConfigManagerから読み込みます
        /// </summary>
        private static void LoadLanguageSettings()
        {
            try
            {
                // ConfigManagerから設定を読み込み（統一設定システム）
                ConfigManager.LoadConfig();
                var savedLanguage = ConfigManager.CurrentConfig.CurrentLanguage;
                
                if (!string.IsNullOrEmpty(savedLanguage) && AvailableLanguages.ContainsKey(savedLanguage))
                {
                    CurrentLanguage = savedLanguage;
                }
                else
                {
                    // システム言語を取得
                    var systemCulture = CultureInfo.CurrentUICulture;
                    var systemLanguageCode = GetSupportedLanguageCode(systemCulture);
                    CurrentLanguage = systemLanguageCode;
                    
                    // システム言語をConfigManagerに保存
                    ConfigManager.UpdateLanguage(CurrentLanguage);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load language settings: {ex.Message}");
                CurrentLanguage = "ja-JP"; // デフォルト
            }
        }

        // SaveLanguageSettingsは削除 - ConfigManager.UpdateLanguage()で統一管理

        private static string GetSupportedLanguageCode(CultureInfo culture)
        {
            // 完全一致を確認
            var exactMatch = AvailableLanguages.Keys.FirstOrDefault(lang => 
                lang.Equals(culture.Name, StringComparison.OrdinalIgnoreCase));
            if (exactMatch != null)
                return exactMatch;

            // 言語コードのみで一致を確認（例：en-US → en）
            var languageCode = culture.TwoLetterISOLanguageName;
            var languageMatch = AvailableLanguages.Keys.FirstOrDefault(lang => 
                lang.StartsWith(languageCode + "-", StringComparison.OrdinalIgnoreCase));
            if (languageMatch != null)
                return languageMatch;

            // デフォルトは日本語
            return "ja-JP";
        }

        private static void LoadLanguageFromXml(string languageCode)
        {
            var app = Application.Current;
            if (app == null) return;

            // 言語コードを短縮形に変換（ja-JP -> ja）
            var shortLanguageCode = languageCode.Split('-')[0];
            
            var xmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Languages", "Language.xml");
            
            if (!File.Exists(xmlPath))
            {
                throw new FileNotFoundException($"Language file not found: {xmlPath}");
            }

            var doc = XDocument.Load(xmlPath);
            var languagesRoot = doc.Root;

            var resourceDict = new ResourceDictionary();

            foreach (var keyElement in languagesRoot.Elements())
            {
                var keyName = keyElement.Name.LocalName;
                var languageElement = keyElement.Element(shortLanguageCode);
                
                if (languageElement != null)
                {
                    resourceDict[keyName] = languageElement.Value;
                }
                else
                {
                    // フォールバック：日本語を使用
                    var fallbackElement = keyElement.Element("ja");
                    if (fallbackElement != null)
                    {
                        resourceDict[keyName] = fallbackElement.Value;
                    }
                }
            }

            app.Resources.MergedDictionaries.Add(resourceDict);
        }

        /// <summary>
        /// 指定された言語コードの表示名を取得します
        /// </summary>
        /// <param name="languageCode">言語コード</param>
        /// <returns>言語の表示名</returns>
        public static string GetLanguageDisplayName(string languageCode)
        {
            if (AvailableLanguages.TryGetValue(languageCode, out var resourceKey))
            {
                var app = Application.Current;
                if (app?.Resources[resourceKey] is string displayName)
                {
                    return displayName;
                }
            }
            return languageCode;
        }

        /// <summary>
        /// 指定されたキーの多言語文字列を取得します
        /// </summary>
        /// <param name="key">リソースキー</param>
        /// <returns>現在の言語の文字列、見つからない場合はキー名</returns>
        public static string GetString(string key)
        {
            var app = Application.Current;
            if (app?.Resources[key] is string value)
            {
                return value;
            }
            return key; // フォールバック
        }
        
        #endregion
    }
}