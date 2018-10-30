namespace TailBlazer.Views.Formatting
{
    using DynamicData;
    using TailBlazer.Domain.FileHandling.Search;
    public interface IIconProvider
    {
        #region Properties
        IDefaultIconSelector DefaultIconSelector { get; }
        IObservableList<IconDescription> Icons { get; }
        #endregion
    }
}