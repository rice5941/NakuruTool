using System.Collections.Generic;
using NakuruTool.ViewModels.Beatmap;

namespace NakuruTool.ViewModels.Collection
{
    /// <summary>
    /// コレクション選択時のビートマップ一覧表示ViewModel
    /// </summary>
    public class CollectionBeatmapListViewModel : BeatmapListViewModelBase
    {
        #region コンストラクタ
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public CollectionBeatmapListViewModel() : base()
        {
            _currentCollection = null;
        }
        
        #endregion

        #region プロパティ

        /// <summary>
        /// 現在表示中のコレクション
        /// </summary>
        public CollectionViewModel CurrentCollection
        {
            get { return _currentCollection; }
            private set { RaisePropertyChangedIfSet(ref _currentCollection, value); }
        }

        #endregion

        #region 関数

        /// <summary>
        /// 選択されたコレクションのビートマップを表示します
        /// </summary>
        /// <param name="collection">選択されたコレクション</param>
        public void ShowCollectionBeatmaps(CollectionViewModel collection)
        {
            CurrentCollection = collection;
            
            if (collection != null)
            {
                SetBeatmaps(collection.Beatmaps);
            }
            else
            {
                SetBeatmaps(new List<BeatmapViewModel>());
            }
        }

        #endregion

        #region メンバ変数

        /// <summary>
        /// 現在表示中のコレクション
        /// </summary>
        private CollectionViewModel _currentCollection;

        #endregion
    }
}