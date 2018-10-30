namespace TailBlazer.Domain.FileHandling.Recent
{
    using DynamicData;
    public interface IRecentFileCollection
    {
        #region Properties
        IObservableList<RecentFile> Items { get; }
        #endregion
        #region Methods
        void Add(RecentFile file);
        void Remove(RecentFile file);
        #endregion
    }
}