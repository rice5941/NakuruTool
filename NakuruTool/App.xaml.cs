using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;
using System.Threading.Tasks;
using System.Diagnostics;

using Livet;
using NakuruTool.Services;
using NakuruTool.Models.Theme;

namespace NakuruTool
{
    /// <summary>
    /// アプリケーションのメインクラス
    /// </summary>
    public partial class App : Application
    {
        #region プロパティ
        
        /// <summary>
        /// テーマドメイン
        /// </summary>
        public static ThemeDomain ThemeDomain { get; private set; }
        
        /// <summary>
        /// 現在のテーマがダークテーマかどうか
        /// </summary>
        public static bool IsDarkTheme 
        { 
            get { return ThemeDomain?.IsDarkTheme ?? false; } 
        }
        
        /// <summary>
        /// 現在のテーマタイプ
        /// </summary>
        public static ThemeType CurrentThemeType
        {
            get { return ThemeDomain?.CurrentTheme?.CurrentThemeType ?? ThemeType.Light; }
        }
        
        #endregion

        #region 関数
        
        /// <summary>
        /// アプリケーション起動時の初期化処理
        /// </summary>
        /// <param name="sender">イベント送信者</param>
        /// <param name="e">イベント引数</param>
        private void Application_Startup(object sender, StartupEventArgs e)
        {
            DispatcherHelper.UIDispatcher = Dispatcher;
            
            // 設定ファイルを読み込み
            ConfigManager.LoadConfig();
            
            // テーマドメインを初期化
            ThemeDomain = new ThemeDomain();
            ThemeDomain.Initialize();
            
            // 保存されたテーマをアプリケーションに適用
            ThemeDomain.ApplyThemeToApplication();
            
            // 多言語対応の初期化（保存された言語で）
            LanguageManager.Initialize(ConfigManager.CurrentConfig.CurrentLanguage);
            
            // PLINQウォームアップの実行（JITコンパイル遅延検証用）
            WarmupPlinq();
        }

        /// <summary>
        /// テーマを切り替えます
        /// </summary>
        public static void SwitchTheme()
        {
            if (ThemeDomain != null)
            {
                ThemeDomain.SwitchTheme();
                ThemeDomain.ApplyThemeToApplication();
            }
        }

        /// <summary>
        /// 指定されたテーマタイプを設定します
        /// </summary>
        /// <param name="themeType">テーマタイプ</param>
        public static void SetTheme(ThemeType themeType)
        {
            if (ThemeDomain != null)
            {
                ThemeDomain.SetTheme(themeType);
                ThemeDomain.ApplyThemeToApplication();
            }
        }

        /// <summary>
        /// PLINQのウォームアップを行い、JITコンパイル遅延を解消します
        /// </summary>
        private void WarmupPlinq()
        {
            try
            {
                // ダミーデータでParallel.ForEachを実行（ExportStoreと同じパターン）
                var dummyData = Enumerable.Range(1, 10000).ToList();
                var result = new System.Collections.Concurrent.ConcurrentDictionary<int, string>();

                Parallel.ForEach(dummyData, new ParallelOptions
                {
                    MaxDegreeOfParallelism = Environment.ProcessorCount
                }, item =>
                {
                    result.TryAdd(item, $"dummy_{item}");
                });

                // AsParallel()も実行（ExportStoreのProcessBeatmapsAsyncと同じパターン）
                var parallelResult = dummyData.AsParallel()
                    .WithDegreeOfParallelism(Environment.ProcessorCount)
                    .Select(x => new { id = x, value = $"parallel_{x}" })
                    .ToArray();
            }
            catch (Exception ex)
            {
                // エラーログのみ保持（本番環境での問題診断用）
                Debug.WriteLine($"[PLINQウォームアップ] エラー: {ex.Message}");
            }
        }
        
        #endregion
    }
}
