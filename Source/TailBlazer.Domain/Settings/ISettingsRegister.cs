namespace TailBlazer.Domain.Settings
{
    using TailBlazer.Domain.Annotations;
    public interface ISettingsRegister
    {
        #region Methods
        void Register<T>([NotNull] IConverter<T> converter, [NotNull] string key);
        #endregion
    }
}