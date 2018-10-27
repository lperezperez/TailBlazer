namespace TailBlazer.Domain.FileHandling.Search
{
    using System;
    using DynamicData;
    public interface ISearchInfoCollection : IDisposable
    {
        #region Properties
        IObservable<ILineProvider> All { get; }
        IObservableCache<SearchInfo, string> Searches { get; }
        #endregion
        #region Methods
        void Add(string searchText, bool useRegex);
        void Remove(string searchText);
        #endregion
    }
}