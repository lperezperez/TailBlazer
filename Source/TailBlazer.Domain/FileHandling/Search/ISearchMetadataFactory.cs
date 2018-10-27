namespace TailBlazer.Domain.FileHandling.Search
{
    using TailBlazer.Domain.Annotations;
    public interface ISearchMetadataFactory
    {
        #region Methods
        SearchMetadata Create([NotNull] string searchText, bool useRegex, int index, bool filter, bool isGlobal = false);
        #endregion
    }
}