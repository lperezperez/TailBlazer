namespace TailBlazer.Domain.FileHandling.Search
{
    public interface IGlobalSearchOptions
    {
        #region Properties
        ISearchMetadataCollection Metadata { get; }
        #endregion
    }
}