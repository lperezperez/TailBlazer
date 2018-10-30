namespace TailBlazer.Infrastucture
{
    using System.Collections.Generic;
    using DynamicData.Kernel;
    using TailBlazer.Domain.Infrastructure;
    using TailBlazer.Views;
    public class ViewFactoryService : IViewFactoryRegister, IViewFactoryProvider
    {
        #region Fields
        private readonly IObjectProvider _objectProvider;
        private readonly IDictionary<string, IViewModelFactory> _viewFactories = new Dictionary<string, IViewModelFactory>();
        #endregion
        #region Constructors
        public ViewFactoryService(IObjectProvider objectProvider) { this._objectProvider = objectProvider; }
        #endregion
        #region Methods
        public Optional<IViewModelFactory> Lookup(string key) => this._viewFactories.Lookup(key);
        public void Register<T>()
            where T : IViewModelFactory
        {
            var register = (IViewModelFactory)this._objectProvider.Get<T>();
            this._viewFactories[register.Key] = register;
        }
        #endregion
    }
}