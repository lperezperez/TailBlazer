namespace TailBlazer.Domain.Settings
{
    public interface ISettingFactory
    {
        #region Methods
        ISetting<T> Create<T>(IConverter<T> converter, string key);
        #endregion
    }
}