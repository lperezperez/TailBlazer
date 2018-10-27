namespace TailBlazer.Domain.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using DynamicData;
    using TailBlazer.Domain.FileHandling.Search;
    public class LineMatches : ILineMatches
    {
        #region Fields
        private readonly IObservable<IEnumerable<SearchMetadata>> _strings;
        #endregion
        #region Constructors
        public LineMatches(ICombinedSearchMetadataCollection searchMetadataCollection) { this._strings = searchMetadataCollection.Combined.Connect().IgnoreUpdateWhen((current, previous) => SearchMetadata.EffectsHighlightComparer.Equals(current, previous)).QueryWhenChanged(query => query.Items.OrderBy(si => si.Position)).Replay(1).RefCount(); }
        #endregion
        #region Methods
        public IObservable<LineMatchCollection> GetMatches(string inputText)
        {
            return this._strings.Select
                (
                 meta =>
                     {
                         //build list of matching filters
                         return new LineMatchCollection(meta.OrderBy(m => m.Position).Where(m => m.Predicate(inputText)).Select(m => new LineMatch(m)).ToArray());
                     }).StartWith(LineMatchCollection.Empty);
        }
        #endregion
    }
}