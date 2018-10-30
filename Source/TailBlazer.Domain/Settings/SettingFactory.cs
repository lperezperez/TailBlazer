namespace TailBlazer.Domain.Settings
{
    using System;
    using TailBlazer.Domain.Infrastructure;
    public class SettingFactory : ISettingFactory
    {
        #region Fields
        private readonly ILogFactory _logFactory;
        private readonly IObjectProvider _objectProvider;
        private readonly ISettingsStore _settingsStore;
        #endregion
        #region Constructors
        public SettingFactory(IObjectProvider objectProvider, ILogFactory logFactory, ISettingsStore settingsStore)
        {
            if (objectProvider == null) throw new ArgumentNullException(nameof(objectProvider));
            if (logFactory == null) throw new ArgumentNullException(nameof(logFactory));
            this._objectProvider = objectProvider;
            this._logFactory = logFactory;
            this._settingsStore = settingsStore;
        }
        #endregion
        #region Methods
        public ISetting<T> Create<T>(IConverter<T> converter, string key)
        {
            //TODO: Cache stored setting and retrive if required elsewhere
            var setting = new Setting<T>(this._logFactory.Create<T>(), this._settingsStore, converter, key);
            return setting;
        }
        #endregion
    }
}