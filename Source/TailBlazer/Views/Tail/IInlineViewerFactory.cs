namespace TailBlazer.Views.Tail
{
    using System;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.FileHandling.Search;
    public interface IInlineViewerFactory
    {
        #region Methods
        InlineViewer Create(ICombinedSearchMetadataCollection combinedSearchMetadataCollection, IObservable<ILineProvider> lineProvider, IObservable<LineProxy> selectedChanged);
        #endregion
    }
}