using System;
using System.Globalization;
using System.Windows.Data;

namespace NakuruTool.Converters
{
    /// <summary>
    /// 複数のViewで共通利用されるValueConverterを集約したクラス
    /// </summary>
    public static class CommonConverter
    {
        #region BooleanInverseConverter

        /// <summary>
        /// Boolean値を反転するConverter
        /// </summary>
        public static readonly IValueConverter BooleanInverse = new BooleanInverseConverter();

        /// <summary>
        /// Boolean値を反転するConverterの実装
        /// </summary>
        private class BooleanInverseConverter : IValueConverter
        {
            /// <summary>
            /// Boolean値を反転します
            /// </summary>
            /// <param name="value">変換元の値</param>
            /// <param name="targetType">変換先の型</param>
            /// <param name="parameter">変換パラメータ</param>
            /// <param name="culture">カルチャ情報</param>
            /// <returns>反転されたBoolean値</returns>
            public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool boolValue)
                {
                    return !boolValue;
                }
                return false;
            }

            /// <summary>
            /// Boolean値を反転します（逆変換）
            /// </summary>
            /// <param name="value">変換元の値</param>
            /// <param name="targetType">変換先の型</param>
            /// <param name="parameter">変換パラメータ</param>
            /// <param name="culture">カルチャ情報</param>
            /// <returns>反転されたBoolean値</returns>
            public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            {
                if (value is bool boolValue)
                {
                    return !boolValue;
                }
                return false;
            }
        }

        #endregion

        #region 今後追加予定のConverter

        // 今後、他のConverterもここに追加していきます
        // 例：
        // public static readonly IValueConverter StringToVisibility = new StringToVisibilityConverter();
        // public static readonly IValueConverter NullToBool = new NullToBoolConverter();

        #endregion
    }
}