namespace TailBlazer.Infrastucture
{
    using TailBlazer.Views;
    public interface IViewFactoryRegister
    {
        #region Methods
        void Register<T>()
            where T : IViewModelFactory;
        #endregion
    }
}