using System.Collections.Generic;
using System.Linq;
using Livet;
using NakuruTool.Models.Collection;
using NakuruTool.ViewModels.Beatmap;

namespace NakuruTool.ViewModels.Collection
{
    /// <summary>
    /// View用のコレクション情報を表現するViewModel
    /// </summary>
    public class CollectionViewModel : ViewModel
    {
        #region プロパティ

        /// <summary>
        /// コレクション名
        /// </summary>
        public string Name
        {
            get { return _name; }
            set { RaisePropertyChangedIfSet(ref _name, value); }
        }

        /// <summary>
        /// ビートマップリスト
        /// </summary>
        public List<BeatmapViewModel> Beatmaps
        {
            get { return _beatmaps; }
            set { RaisePropertyChangedIfSet(ref _beatmaps, value, nameof(BeatmapCount)); }
        }

        /// <summary>
        /// ビートマップ数
        /// </summary>
        public int BeatmapCount => _beatmaps.Count;

        /// <summary>
        /// エクスポート用に選択されているかどうか
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set { RaisePropertyChangedIfSet(ref _isSelected, value); }
        }

        /// <summary>
        /// 元のCollectionデータへの参照
        /// </summary>
        public Models.Collection.Collection OriginalCollection
        {
            get { return _originalCollection; }
            set { RaisePropertyChangedIfSet(ref _originalCollection, value); }
        }
        
        #endregion


        #region 関数

        /// <summary>
        /// Collectionデータクラスから変換します
        /// </summary>
        /// <param name="collection">変換元のCollection</param>
        /// <returns>CollectionViewModel</returns>
        public static CollectionViewModel FromCollection(Models.Collection.Collection collection)
        {
            return new CollectionViewModel
            {
                Name = collection.Name,
                Beatmaps = collection.Beatmaps.Select(BeatmapViewModel.FromBeatmap).ToList(),
                OriginalCollection = collection,
                IsSelected = false
            };
        }

        #endregion
        
        #region メンバ変数
        
        /// <summary>
        /// コレクション名
        /// </summary>
        private string _name = string.Empty;
        
        /// <summary>
        /// ビートマップリスト
        /// </summary>
        private List<BeatmapViewModel> _beatmaps = new List<BeatmapViewModel>();

        /// <summary>
        /// エクスポート用に選択されているかどうか
        /// </summary>
        private bool _isSelected = false;

        /// <summary>
        /// 元のCollectionデータへの参照
        /// </summary>
        private Models.Collection.Collection _originalCollection;
        
        #endregion
    }
}