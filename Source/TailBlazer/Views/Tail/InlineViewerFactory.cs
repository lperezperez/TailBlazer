namespace TailBlazer.Views.Tail
{
    using System;
    using TailBlazer.Domain.FileHandling;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.Infrastructure;
    public class InlineViewerFactory : IInlineViewerFactory
    {
        #region Fields
        private readonly IObjectProvider _objectProvider;
        #endregion
        #region Constructors
        public InlineViewerFactory(IObjectProvider objectProvider) { this._objectProvider = objectProvider; }
        #endregion
        #region Methods
        public InlineViewer Create(ICombinedSearchMetadataCollection combinedSearchMetadataCollection, IObservable<ILineProvider> lineProvider, IObservable<LineProxy> selectedChanged)
        {
            var args = new IArgument[] { new Argument<IObservable<ILineProvider>>(lineProvider), new Argument<IObservable<LineProxy>>(selectedChanged), new Argument<ICombinedSearchMetadataCollection>(combinedSearchMetadataCollection) };
            return this._objectProvider.Get<InlineViewer>(args);
        }
        #endregion
    }
}