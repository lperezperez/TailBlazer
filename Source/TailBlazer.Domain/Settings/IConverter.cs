namespace TailBlazer.Domain.Settings
{
    public interface IConverter<T>
    {
        #region Methods
        T Convert(State state);
        State Convert(T state);
        T GetDefaultValue();
        #endregion
    }
}