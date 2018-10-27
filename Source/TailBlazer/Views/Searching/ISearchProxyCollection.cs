namespace TailBlazer.Views.Searching
{
    using System;
    using System.Collections.ObjectModel;
    using Dragablz;
    using TailBlazer.Domain.Infrastructure;
    public interface ISearchProxyCollection : IDisposable
    {
        #region Properties
        IProperty<int> Count { get; }
        ReadOnlyObservableCollection<SearchOptionsProxy> Excluded { get; }
        ReadOnlyObservableCollection<SearchOptionsProxy> Included { get; }
        VerticalPositionMonitor PositionMonitor { get; }
        #endregion
    }
}