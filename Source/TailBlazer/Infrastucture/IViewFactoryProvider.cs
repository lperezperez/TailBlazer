namespace TailBlazer.Infrastucture
{
    using DynamicData.Kernel;
    using TailBlazer.Views;
    public interface IViewFactoryProvider
    {
        #region Methods
        Optional<IViewModelFactory> Lookup(string key);
        #endregion
    }
}