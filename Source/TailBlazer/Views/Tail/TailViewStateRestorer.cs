namespace TailBlazer.Views.Tail
{
    using System.Linq;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Domain.Settings;
    using TailBlazer.Views.Searching;
    public class TailViewStateRestorer : ITailViewStateRestorer
    {
        #region Fields
        private readonly ILogger _logger;
        private readonly ISearchStateToMetadataMapper _searchStateToMetadataMapper;
        #endregion
        #region Constructors
        public TailViewStateRestorer(ILogger logger, ISearchStateToMetadataMapper searchStateToMetadataMapper)
        {
            this._logger = logger;
            this._searchStateToMetadataMapper = searchStateToMetadataMapper;
        }
        #endregion
        #region Methods
        public void Restore(TailViewModel view, State state)
        {
            var converter = new TailViewToStateConverter();
            this.Restore(view, converter.Convert(state));
        }
        public void Restore(TailViewModel view, TailViewState tailviewstate)
        {
            this._logger.Info("Applying {0} saved search settings  for {1} ", tailviewstate.SearchItems.Count(), view.Name);
            var searches = tailviewstate.SearchItems.Select(state => this._searchStateToMetadataMapper.Map(state, false));
            view.SearchMetadataCollection.Add(searches);
            view.SearchCollection.Select(tailviewstate.SelectedSearch);
            this._logger.Info("DONE: Applied {0} search settings for {1} ", tailviewstate.SearchItems.Count(), view.Name);
        }
        #endregion
    }
}