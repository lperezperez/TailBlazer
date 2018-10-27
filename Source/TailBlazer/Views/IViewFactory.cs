namespace TailBlazer.Views
{
    using TailBlazer.Domain.Settings;
    using TailBlazer.Infrastucture;
    public interface IViewModelFactory
    {
        #region Properties
        string Key { get; }
        #endregion
        #region Methods
        HeaderedView Create(ViewState state);
        #endregion
    }
}