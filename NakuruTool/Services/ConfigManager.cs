using System;
using System.IO;
using System.Text.Json;

namespace NakuruTool.Services
{
    /// <summary>
    /// アプリケーション設定データを格納するクラス
    /// </summary>
    public class AppConfig
    {
        /// <summary>
        /// ダークテーマを使用するかどうか
        /// </summary>
        public bool IsDarkTheme { get; set; } = false;
        
        /// <summary>
        /// 現在の言語設定
        /// </summary>
        public string CurrentLanguage { get; set; } = "ja-JP";
    }

    /// <summary>
    /// アプリケーション設定を管理するクラス
    /// </summary>
    public static class ConfigManager
    {
        #region メンバ変数
        
        /// <summary>
        /// 設定ファイルのパス
        /// </summary>
        private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");
        
        /// <summary>
        /// 現在の設定オブジェクト
        /// </summary>
        private static AppConfig _currentConfig;
        
        #endregion
        
        #region プロパティ
        
        /// <summary>
        /// 現在の設定を取得します
        /// </summary>
        public static AppConfig CurrentConfig 
        { 
            get 
            { 
                if (_currentConfig == null)
                {
                    LoadConfig();
                }
                return _currentConfig; 
            } 
        }
        
        #endregion

        #region 関数
        
        /// <summary>
        /// 設定ファイルから設定を読み込みます
        /// </summary>
        public static void LoadConfig()
        {
            try
            {
                if (File.Exists(ConfigFilePath))
                {
                    // 既存の設定ファイルを読み込み
                    var jsonString = File.ReadAllText(ConfigFilePath);
                    _currentConfig = JsonSerializer.Deserialize<AppConfig>(jsonString);
                    
                    if (_currentConfig == null)
                    {
                        _currentConfig = new AppConfig();
                    }
                }
                else
                {
                    // 設定ファイルが存在しない場合はデフォルト設定を作成
                    _currentConfig = new AppConfig();
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                // 設定ファイル読み込み失敗時はデフォルト設定を使用
                System.Diagnostics.Debug.WriteLine($"Failed to load config: {ex.Message}");
                _currentConfig = new AppConfig();
            }
        }

        /// <summary>
        /// 現在の設定をファイルに保存します
        /// </summary>
        public static void SaveConfig()
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true // 見やすい形式で保存
                };
                
                var jsonString = JsonSerializer.Serialize(_currentConfig, options);
                File.WriteAllText(ConfigFilePath, jsonString);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to save config: {ex.Message}");
            }
        }

        /// <summary>
        /// テーマ設定を更新し、設定ファイルに保存します
        /// </summary>
        /// <param name="isDarkTheme">ダークテーマを使用するかどうか</param>
        public static void UpdateTheme(bool isDarkTheme)
        {
            CurrentConfig.IsDarkTheme = isDarkTheme;
            SaveConfig();
        }

        /// <summary>
        /// 言語設定を更新し、設定ファイルに保存します
        /// </summary>
        /// <param name="languageCode">言語コード</param>
        public static void UpdateLanguage(string languageCode)
        {
            CurrentConfig.CurrentLanguage = languageCode;
            SaveConfig();
        }
        
        #endregion
    }
}