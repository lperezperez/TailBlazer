namespace TailBlazer.Domain.Infrastructure
{
    public interface ILogFactory
    {
        #region Methods
        ILogger Create(string name);
        ILogger Create<T>();
        #endregion
    }
}