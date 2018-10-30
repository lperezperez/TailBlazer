namespace TailBlazer.Infrastucture.KeyboardNavigation
{
    public interface IPageProvider
    {
        #region Properties
        int FirstIndex { get; }
        int PageSize { get; }
        #endregion
    }
}