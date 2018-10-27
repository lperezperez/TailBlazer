namespace TailBlazer.Views.Tail
{
    using TailBlazer.Domain.Settings;
    public interface ITailViewStateRestorer
    {
        #region Methods
        void Restore(TailViewModel view, State state);
        void Restore(TailViewModel view, TailViewState tailviewstate);
        #endregion
    }
}