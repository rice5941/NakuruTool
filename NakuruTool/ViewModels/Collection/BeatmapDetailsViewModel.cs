using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Livet;
using Livet.Commands;
using NakuruTool.Models;

namespace NakuruTool.ViewModels.Collection
{
    /// <summary>
    /// ビートマップ詳細表示のViewModel
    /// </summary>
    public class BeatmapDetailsViewModel : NotificationBase
    {
        #region コンストラクタ
        
        /// <summary>
        /// コンストラクタ
        /// </summary>
        public BeatmapDetailsViewModel()
        {
            _beatmaps = new ObservableCollection<BeatmapViewModel>();
            _allBeatmaps = new List<BeatmapViewModel>();
            _filteredBeatmaps = new List<BeatmapViewModel>();
            _beatmapFilterText = string.Empty;
            _currentPage = 1;
            _itemsPerPage = 50; // デフォルト50件表示
            _totalPages = 0;
            _pagingInfo = "0件";
        }
        
        #endregion

        #region プロパティ

        /// <summary>
        /// 表示中のビートマップ一覧
        /// </summary>
        public ObservableCollection<BeatmapViewModel> Beatmaps
        {
            get { return _beatmaps; }
            set { SetProperty(ref _beatmaps, value); }
        }

        /// <summary>
        /// 全ビートマップ一覧（ページング用）
        /// </summary>
        public List<BeatmapViewModel> AllBeatmaps
        {
            get { return _allBeatmaps; }
            set
            {
                if (SetProperty(ref _allBeatmaps, value))
                {
                    ApplyBeatmapFilter();
                }
            }
        }

        /// <summary>
        /// フィルタリングされたビートマップ一覧
        /// </summary>
        public List<BeatmapViewModel> FilteredBeatmaps
        {
            get { return _filteredBeatmaps; }
            set
            {
                if (SetProperty(ref _filteredBeatmaps, value))
                {
                    UpdatePaging();
                }
            }
        }

        /// <summary>
        /// ビートマップ検索テキスト
        /// </summary>
        public string BeatmapFilterText
        {
            get { return _beatmapFilterText; }
            set
            {
                if (SetProperty(ref _beatmapFilterText, value))
                {
                    ApplyBeatmapFilter();
                }
            }
        }

        /// <summary>
        /// 現在のページ番号（1から開始）
        /// </summary>
        public int CurrentPage
        {
            get { return _currentPage; }
            set
            {
                if (SetProperty(ref _currentPage, value))
                {
                    UpdatePagedBeatmaps();
                    UpdatePagingInfo();
                }
            }
        }

        /// <summary>
        /// 1ページあたりの表示件数
        /// </summary>
        public int ItemsPerPage
        {
            get { return _itemsPerPage; }
            set
            {
                if (SetProperty(ref _itemsPerPage, value))
                {
                    UpdatePaging();
                }
            }
        }

        /// <summary>
        /// 総ページ数
        /// </summary>
        public int TotalPages
        {
            get { return _totalPages; }
            set { SetProperty(ref _totalPages, value); }
        }

        /// <summary>
        /// ページング情報表示用テキスト
        /// </summary>
        public string PagingInfo
        {
            get { return _pagingInfo; }
            set { SetProperty(ref _pagingInfo, value); }
        }

        /// <summary>
        /// 前のページに移動するコマンド
        /// </summary>
        public ViewModelCommand PreviousPageCommand
        {
            get
            {
                if (_previousPageCommand == null)
                {
                    _previousPageCommand = new ViewModelCommand(PreviousPage, CanPreviousPage);
                }
                return _previousPageCommand;
            }
        }

        /// <summary>
        /// 次のページに移動するコマンド
        /// </summary>
        public ViewModelCommand NextPageCommand
        {
            get
            {
                if (_nextPageCommand == null)
                {
                    _nextPageCommand = new ViewModelCommand(NextPage, CanNextPage);
                }
                return _nextPageCommand;
            }
        }

        /// <summary>
        /// 最初のページに移動するコマンド
        /// </summary>
        public ViewModelCommand FirstPageCommand
        {
            get
            {
                if (_firstPageCommand == null)
                {
                    _firstPageCommand = new ViewModelCommand(FirstPage, CanFirstPage);
                }
                return _firstPageCommand;
            }
        }

        /// <summary>
        /// 最後のページに移動するコマンド
        /// </summary>
        public ViewModelCommand LastPageCommand
        {
            get
            {
                if (_lastPageCommand == null)
                {
                    _lastPageCommand = new ViewModelCommand(LastPage, CanLastPage);
                }
                return _lastPageCommand;
            }
        }

        #endregion

        #region 関数

        /// <summary>
        /// ビートマップ一覧を設定します
        /// </summary>
        /// <param name="beatmaps">ビートマップ一覧</param>
        public void SetBeatmaps(IEnumerable<BeatmapViewModel> beatmaps)
        {
            AllBeatmaps = beatmaps?.ToList() ?? new List<BeatmapViewModel>();
        }

        /// <summary>
        /// ページング情報を更新します
        /// </summary>
        private void UpdatePaging()
        {
            if (FilteredBeatmaps == null || FilteredBeatmaps.Count == 0)
            {
                TotalPages = 0;
                CurrentPage = 1;
                UpdatePagedBeatmaps();
                UpdatePagingInfo();
                return;
            }

            TotalPages = (int)Math.Ceiling((double)FilteredBeatmaps.Count / ItemsPerPage);
            if (CurrentPage > TotalPages)
            {
                CurrentPage = Math.Max(1, TotalPages);
            }
            UpdatePagedBeatmaps();
            UpdatePagingInfo();
        }

        /// <summary>
        /// 現在のページに表示するビートマップを更新します
        /// </summary>
        private void UpdatePagedBeatmaps()
        {
            Beatmaps.Clear();

            if (FilteredBeatmaps == null || FilteredBeatmaps.Count == 0)
            {
                return;
            }

            var startIndex = (CurrentPage - 1) * ItemsPerPage;
            var endIndex = Math.Min(startIndex + ItemsPerPage, FilteredBeatmaps.Count);

            for (int i = startIndex; i < endIndex; i++)
            {
                Beatmaps.Add(FilteredBeatmaps[i]);
            }

            // コマンドの有効性を更新
            PreviousPageCommand?.RaiseCanExecuteChanged();
            NextPageCommand?.RaiseCanExecuteChanged();
            FirstPageCommand?.RaiseCanExecuteChanged();
            LastPageCommand?.RaiseCanExecuteChanged();
        }

        /// <summary>
        /// ページング情報テキストを更新します
        /// </summary>
        private void UpdatePagingInfo()
        {
            if (FilteredBeatmaps == null || FilteredBeatmaps.Count == 0)
            {
                PagingInfo = "0件";
                return;
            }

            var startIndex = (CurrentPage - 1) * ItemsPerPage + 1;
            var endIndex = Math.Min(CurrentPage * ItemsPerPage, FilteredBeatmaps.Count);
            
            // フィルタが適用されている場合は、全体数も表示
            if (string.IsNullOrWhiteSpace(BeatmapFilterText))
            {
                PagingInfo = $"{startIndex}-{endIndex} / {FilteredBeatmaps.Count}件 (ページ {CurrentPage}/{TotalPages})";
            }
            else
            {
                var totalCount = AllBeatmaps?.Count ?? 0;
                PagingInfo = $"{startIndex}-{endIndex} / {FilteredBeatmaps.Count}件 (全{totalCount}件中) (ページ {CurrentPage}/{TotalPages})";
            }
        }

        /// <summary>
        /// 前のページに移動します
        /// </summary>
        private void PreviousPage()
        {
            if (CanPreviousPage())
            {
                CurrentPage--;
            }
        }

        /// <summary>
        /// 前のページに移動できるかどうか
        /// </summary>
        /// <returns></returns>
        private bool CanPreviousPage()
        {
            return CurrentPage > 1;
        }

        /// <summary>
        /// 次のページに移動します
        /// </summary>
        private void NextPage()
        {
            if (CanNextPage())
            {
                CurrentPage++;
            }
        }

        /// <summary>
        /// 次のページに移動できるかどうか
        /// </summary>
        /// <returns></returns>
        private bool CanNextPage()
        {
            return CurrentPage < TotalPages;
        }

        /// <summary>
        /// 最初のページに移動します
        /// </summary>
        private void FirstPage()
        {
            if (CanFirstPage())
            {
                CurrentPage = 1;
            }
        }

        /// <summary>
        /// 最初のページに移動できるかどうか
        /// </summary>
        /// <returns></returns>
        private bool CanFirstPage()
        {
            return CurrentPage > 1 && TotalPages > 0;
        }

        /// <summary>
        /// 最後のページに移動します
        /// </summary>
        private void LastPage()
        {
            if (CanLastPage())
            {
                CurrentPage = TotalPages;
            }
        }

        /// <summary>
        /// 最後のページに移動できるかどうか
        /// </summary>
        /// <returns></returns>
        private bool CanLastPage()
        {
            return CurrentPage < TotalPages && TotalPages > 0;
        }

        /// <summary>
        /// ビートマップフィルタを適用します
        /// </summary>
        private void ApplyBeatmapFilter()
        {
            if (AllBeatmaps == null)
            {
                FilteredBeatmaps = new List<BeatmapViewModel>();
                return;
            }

            List<BeatmapViewModel> newFilteredList;
            if (string.IsNullOrWhiteSpace(BeatmapFilterText))
            {
                // フィルタが空の場合は全て表示
                newFilteredList = new List<BeatmapViewModel>(AllBeatmaps);
            }
            else
            {
                // フィルタテキストに一致するビートマップのみ表示（ToLowerInvariant使用）
                var filterLower = BeatmapFilterText.ToLowerInvariant();
                newFilteredList = AllBeatmaps.Where(beatmap =>
                    beatmap.Title.ToLowerInvariant().Contains(filterLower) ||
                    beatmap.Artist.ToLowerInvariant().Contains(filterLower) ||
                    beatmap.Creator.ToLowerInvariant().Contains(filterLower) ||
                    beatmap.Version.ToLowerInvariant().Contains(filterLower)
                ).ToList();
            }

            // フィルタ適用時は最初のページに戻る
            _currentPage = 1;
            FilteredBeatmaps = newFilteredList;
        }

        #endregion

        #region メンバ変数

        /// <summary>
        /// 表示中のビートマップ一覧
        /// </summary>
        private ObservableCollection<BeatmapViewModel> _beatmaps;

        /// <summary>
        /// 全ビートマップ一覧（ページング用）
        /// </summary>
        private List<BeatmapViewModel> _allBeatmaps;

        /// <summary>
        /// フィルタリングされたビートマップ一覧
        /// </summary>
        private List<BeatmapViewModel> _filteredBeatmaps;

        /// <summary>
        /// ビートマップ検索テキスト
        /// </summary>
        private string _beatmapFilterText;

        /// <summary>
        /// 現在のページ番号
        /// </summary>
        private int _currentPage;

        /// <summary>
        /// 1ページあたりの表示件数
        /// </summary>
        private int _itemsPerPage;

        /// <summary>
        /// 総ページ数
        /// </summary>
        private int _totalPages;

        /// <summary>
        /// ページング情報表示用テキスト
        /// </summary>
        private string _pagingInfo;

        /// <summary>
        /// 前のページに移動するコマンド
        /// </summary>
        private ViewModelCommand _previousPageCommand;

        /// <summary>
        /// 次のページに移動するコマンド
        /// </summary>
        private ViewModelCommand _nextPageCommand;

        /// <summary>
        /// 最初のページに移動するコマンド
        /// </summary>
        private ViewModelCommand _firstPageCommand;

        /// <summary>
        /// 最後のページに移動するコマンド
        /// </summary>
        private ViewModelCommand _lastPageCommand;

        #endregion
    }
}