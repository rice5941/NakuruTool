using System.Windows;
using System.Windows.Controls;

namespace NakuruTool.Views.Controls
{
    /// <summary>
    /// 検索用テキストボックスの共通UserControl
    /// </summary>
    public partial class SearchTextBox : UserControl
    {
        #region 依存関係プロパティ
        
        /// <summary>
        /// 検索テキストの依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty SearchTextProperty =
            DependencyProperty.Register(
                nameof(SearchText),
                typeof(string),
                typeof(SearchTextBox),
                new FrameworkPropertyMetadata(
                    string.Empty,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

        /// <summary>
        /// プレースホルダーテキストの依存関係プロパティ
        /// </summary>
        public static readonly DependencyProperty PlaceholderTextProperty =
            DependencyProperty.Register(
                nameof(PlaceholderText),
                typeof(string),
                typeof(SearchTextBox),
                new PropertyMetadata("検索..."));

        #endregion

        #region プロパティ

        /// <summary>
        /// 検索テキスト
        /// </summary>
        public string SearchText
        {
            get { return (string)GetValue(SearchTextProperty); }
            set { SetValue(SearchTextProperty, value); }
        }

        /// <summary>
        /// プレースホルダーテキスト
        /// </summary>
        public string PlaceholderText
        {
            get { return (string)GetValue(PlaceholderTextProperty); }
            set { SetValue(PlaceholderTextProperty, value); }
        }

        #endregion

        #region コンストラクタ

        /// <summary>
        /// コンストラクタ
        /// </summary>
        public SearchTextBox()
        {
            InitializeComponent();
        }

        #endregion
    }
}