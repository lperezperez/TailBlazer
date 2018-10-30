namespace TailBlazer.Views.Searching
{
    using DynamicData;
    using TailBlazer.Views.Recent;
    public interface IRecentSearchCollection
    {
        #region Properties
        IObservableList<RecentSearch> Items { get; }
        #endregion
        #region Methods
        void Add(RecentSearch file);
        void Remove(RecentSearch file);
        #endregion
    }
}