namespace TailBlazer.Views.Tail
{
    using System.Collections.Generic;
    using TailBlazer.Views.Searching;
    public sealed class TailViewState
    {
        #region Fields
        public static readonly TailViewState Empty = new TailViewState();
        #endregion
        #region Constructors
        public TailViewState(string fileName, string selectedSearch, IEnumerable<SearchState> searchItems)
        {
            this.FileName = fileName;
            this.SelectedSearch = selectedSearch;
            this.SearchItems = searchItems;
        }
        private TailViewState() { }
        #endregion
        #region Properties
        public string FileName { get; }
        public IEnumerable<SearchState> SearchItems { get; }
        public string SelectedSearch { get; }
        #endregion
    }
}