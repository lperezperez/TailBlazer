namespace TailBlazer.Domain.Formatting
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Linq;
    using DynamicData;
    using TailBlazer.Domain.FileHandling.Search;
    public class TextFormatter : ITextFormatter
    {
        #region Fields
        private readonly IObservable<IEnumerable<SearchMetadata>> _strings;
        #endregion
        #region Constructors
        public TextFormatter(ICombinedSearchMetadataCollection searchMetadataCollection) { this._strings = searchMetadataCollection.Combined.Connect(meta => meta.Highlight && !meta.IsExclusion).IgnoreUpdateWhen((current, previous) => SearchMetadata.EffectsHighlightComparer.Equals(current, previous)).QueryWhenChanged(query => query.Items.OrderBy(m => m.Position)).Replay(1).RefCount(); }
        #endregion
        #region Methods
        public IObservable<IEnumerable<DisplayText>> GetFormatter(string inputText)
        {
            return this._strings.Select
                (
                 meta =>
                     {
                         //split into 2 parts. 1) matching text 2) matching regex
                         return inputText.MatchString(meta).Select(ms => new DisplayText(ms)).ToArray();
                     });
        }
        #endregion
    }
}