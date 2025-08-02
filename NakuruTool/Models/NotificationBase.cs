using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Livet;

namespace NakuruTool.Models
{
    /// <summary>
    /// プロパティ変更通知機能を提供する基底クラス
    /// INotifyPropertyChangedの実装とSetPropertyメソッドによる効率的なプロパティ管理を提供します
    /// </summary>
    public abstract class NotificationBase : NotificationObject
    {
        #region コンストラクタ

        /// <summary>
        /// NotificationBaseクラスの新しいインスタンスを初期化します
        /// </summary>
        protected NotificationBase()
        {
        }

        #endregion

        #region 関数

        /// <summary>
        /// プロパティ値を設定し、値が変更された場合にPropertyChangedイベントを発生させます
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">プロパティの値を格納するフィールドの参照</param>
        /// <param name="value">設定する新しい値</param>
        /// <param name="propertyName">プロパティ名（通常は自動で設定されます）</param>
        /// <returns>値が実際に変更された場合はtrue、そうでなければfalse</returns>
        protected virtual bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            // 現在の値と新しい値が等しいかチェック
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            // 値を更新
            field = value;

            // プロパティ変更イベントを発生
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// プロパティ値を設定し、値が変更された場合にPropertyChangedイベントを発生させます
        /// 変更前の値での処理と変更後の値での処理を実行することができます
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">プロパティの値を格納するフィールドの参照</param>
        /// <param name="value">設定する新しい値</param>
        /// <param name="onChanging">値変更前に実行するアクション（oldValue, newValue）</param>
        /// <param name="onChanged">値変更後に実行するアクション（oldValue, newValue）</param>
        /// <param name="propertyName">プロパティ名（通常は自動で設定されます）</param>
        /// <returns>値が実際に変更された場合はtrue、そうでなければfalse</returns>
        protected virtual bool SetProperty<T>(ref T field, T value, 
            Action<T, T> onChanging, 
            Action<T, T> onChanged = null, 
            [CallerMemberName] string propertyName = null)
        {
            // 現在の値と新しい値が等しいかチェック
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            // 古い値を保存
            T oldValue = field;

            // 変更前処理を実行
            onChanging?.Invoke(oldValue, value);

            // 値を更新
            field = value;

            // プロパティ変更イベントを発生
            RaisePropertyChanged(propertyName);

            // 変更後処理を実行
            onChanged?.Invoke(oldValue, value);

            return true;
        }

        /// <summary>
        /// プロパティ値を設定し、値が変更された場合にPropertyChangedイベントと関連プロパティのイベントを発生させます
        /// </summary>
        /// <typeparam name="T">プロパティの型</typeparam>
        /// <param name="field">プロパティの値を格納するフィールドの参照</param>
        /// <param name="value">設定する新しい値</param>
        /// <param name="dependentProperties">同時に変更通知を行う関連プロパティ名の配列</param>
        /// <param name="propertyName">プロパティ名（通常は自動で設定されます）</param>
        /// <returns>値が実際に変更された場合はtrue、そうでなければfalse</returns>
        protected virtual bool SetProperty<T>(ref T field, T value, 
            string[] dependentProperties, 
            [CallerMemberName] string propertyName = null)
        {
            // 現在の値と新しい値が等しいかチェック
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            // 値を更新
            field = value;

            // プロパティ変更イベントを発生
            RaisePropertyChanged(propertyName);

            // 関連プロパティの変更イベントを発生
            if (dependentProperties != null)
            {
                foreach (string depProperty in dependentProperties)
                {
                    if (string.IsNullOrEmpty(depProperty) == false)
                    {
                        RaisePropertyChanged(depProperty);
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// 複数のプロパティ名に対してPropertyChangedイベントを発生させます
        /// </summary>
        /// <param name="propertyNames">変更通知を行うプロパティ名の配列</param>
        protected virtual void RaiseMultiplePropertyChanged(params string[] propertyNames)
        {
            if (propertyNames != null)
            {
                foreach (string propertyName in propertyNames)
                {
                    if (string.IsNullOrEmpty(propertyName) == false)
                    {
                        RaisePropertyChanged(propertyName);
                    }
                }
            }
        }

        #endregion
    }
}