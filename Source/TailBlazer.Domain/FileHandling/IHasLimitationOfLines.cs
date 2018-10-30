namespace TailBlazer.Domain.FileHandling
{
    public interface IHasLimitationOfLines
    {
        #region Properties
        bool HasReachedLimit { get; }
        int Maximum { get; }
        #endregion
    }
}