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
        /// 現在のテーマタイプ
        /// </summary>
        public string ThemeType { get; set; } = "Light";
        
        /// <summary>
        /// 現在の言語設定
        /// </summary>
        public string CurrentLanguage { get; set; } = "ja-JP";
        
        /// <summary>
        /// osu!フォルダパス
        /// </summary>
        public string OsuFolderPath { get; set; } = string.Empty;
    }

    /// <summary>
    /// アプリケーション設定を管理するクラス
    /// </summary>
    public static class ConfigManager
    {
        #region イベント

        /// <summary>
        /// osu!フォルダパスが変更されたときに発生するイベント
        /// </summary>
        public static event EventHandler<string> OsuFolderPathChanged;

        #endregion

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
                System.Diagnostics.Debug.WriteLine($"[ConfigManager] LoadConfig: ConfigFilePath = '{ConfigFilePath}'");
                if (File.Exists(ConfigFilePath))
                {
                    // 既存の設定ファイルを読み込み
                    var jsonString = File.ReadAllText(ConfigFilePath);
                    System.Diagnostics.Debug.WriteLine($"[ConfigManager] LoadConfig: JSON content = '{jsonString}'");
                    _currentConfig = JsonSerializer.Deserialize<AppConfig>(jsonString);
                    
                    if (_currentConfig == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[ConfigManager] LoadConfig: Deserialized config is null, creating default");
                        _currentConfig = new AppConfig();
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[ConfigManager] LoadConfig: Loaded OsuFolderPath = '{_currentConfig.OsuFolderPath}'");
                    }
                }
                else
                {
                    // 設定ファイルが存在しない場合はデフォルト設定を作成
                    System.Diagnostics.Debug.WriteLine("[ConfigManager] LoadConfig: Config file does not exist, creating default");
                    _currentConfig = new AppConfig();
                    SaveConfig();
                }
            }
            catch (Exception ex)
            {
                // 設定ファイル読み込み失敗時はデフォルト設定を使用
                System.Diagnostics.Debug.WriteLine($"[ConfigManager] LoadConfig: Failed to load config: {ex.Message}");
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
        /// テーマタイプを更新し、設定ファイルに保存します
        /// </summary>
        /// <param name="themeType">テーマタイプ名</param>
        public static void UpdateThemeType(string themeType)
        {
            CurrentConfig.ThemeType = themeType;
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

        /// <summary>
        /// osu!フォルダパスを更新し、設定ファイルに保存します
        /// </summary>
        /// <param name="folderPath">osu!フォルダパス</param>
        public static void UpdateOsuFolderPath(string folderPath)
        {
            var newPath = folderPath ?? string.Empty;
            var oldPath = CurrentConfig.OsuFolderPath ?? string.Empty;
            
            if (oldPath != newPath)
            {
                CurrentConfig.OsuFolderPath = newPath;
                SaveConfig();
                
                // イベントを発火
                OsuFolderPathChanged?.Invoke(null, newPath);
                System.Diagnostics.Debug.WriteLine($"[ConfigManager] OsuFolderPath changed: '{oldPath}' -> '{newPath}'");
            }
        }
        
        #endregion
    }
}