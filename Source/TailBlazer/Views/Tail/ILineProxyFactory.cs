namespace TailBlazer.Views.Tail
{
    using TailBlazer.Domain.FileHandling;
    public interface ILineProxyFactory
    {
        #region Methods
        LineProxy Create(Line line);
        #endregion
    }
}