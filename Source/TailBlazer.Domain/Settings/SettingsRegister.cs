namespace TailBlazer.Domain.Settings
{
    using System;
    using TailBlazer.Domain.Annotations;
    using TailBlazer.Domain.Infrastructure;
    public class SettingsRegister : ISettingsRegister
    {
        #region Fields
        private readonly IObjectRegister _register;
        private readonly ISettingFactory _settingFactory;
        #endregion
        #region Constructors
        public SettingsRegister([NotNull] IObjectRegister register, [NotNull] ISettingFactory settingFactory)
        {
            if (register == null) throw new ArgumentNullException(nameof(register));
            if (settingFactory == null) throw new ArgumentNullException(nameof(settingFactory));
            this._register = register;
            this._settingFactory = settingFactory;
        }
        #endregion
        #region Methods
        public void Register<T>([NotNull] IConverter<T> converter, [NotNull] string key)
        {
            if (converter == null) throw new ArgumentNullException(nameof(converter));
            if (key == null) throw new ArgumentNullException(nameof(key));
            var setting = this._settingFactory.Create(converter, key);
            this._register.Register(setting);
        }
        #endregion
    }
}