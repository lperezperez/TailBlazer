namespace TailBlazer.Domain.Settings
{
    public interface ISettingsStore
    {
        #region Methods
        State Load(string key);
        void Save(string key, State state);
        #endregion
    }
}