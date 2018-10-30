namespace TailBlazer.Views.Searching
{
    using System;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.FileHandling.Search;
    using TailBlazer.Domain.FileHandling.TextAssociations;
    using TailBlazer.Domain.Formatting;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Views.Formatting;
    public class SearchProxyCollectionFactory : ISearchProxyCollectionFactory
    {
        #region Constructors
        public SearchProxyCollectionFactory([NotNull] ISearchMetadataFactory searchMetadataFactory, [NotNull] ISchedulerProvider schedulerProvider, [NotNull] IColourProvider colourProvider, [NotNull] IIconProvider iconsProvider, [NotNull] ITextAssociationCollection textAssociationCollection, [NotNull] IThemeProvider themeProvider)
        {
            this.SearchMetadataFactory = searchMetadataFactory ?? throw new ArgumentNullException(nameof(searchMetadataFactory));
            this.SchedulerProvider = schedulerProvider ?? throw new ArgumentNullException(nameof(schedulerProvider));
            this.ColourProvider = colourProvider ?? throw new ArgumentNullException(nameof(colourProvider));
            this.IconsProvider = iconsProvider ?? throw new ArgumentNullException(nameof(iconsProvider));
            this.TextAssociationCollection = textAssociationCollection ?? throw new ArgumentNullException(nameof(textAssociationCollection));
            this.ThemeProvider = themeProvider ?? throw new ArgumentNullException(nameof(themeProvider));
        }
        #endregion
        #region Properties
        private IColourProvider ColourProvider { get; }
        private IIconProvider IconsProvider { get; }
        private ISchedulerProvider SchedulerProvider { get; }
        private ISearchMetadataFactory SearchMetadataFactory { get; }
        private ITextAssociationCollection TextAssociationCollection { get; }
        private IThemeProvider ThemeProvider { get; }
        #endregion
        #region Methods
        public ISearchProxyCollection Create([NotNull] ISearchMetadataCollection metadataCollection, Guid id, Action<SearchMetadata> changeScopeAction)
        {
            if (metadataCollection == null) throw new ArgumentNullException(nameof(metadataCollection));
            return new SearchProxyCollection(metadataCollection, id, changeScopeAction, this.SchedulerProvider, this.ColourProvider, this.IconsProvider, this.TextAssociationCollection, this.ThemeProvider);
        }
        #endregion
    }
}