namespace TailBlazer.Infrastucture
{
    public interface ISelectedAware
    {
        #region Properties
        bool IsSelected { get; set; }
        #endregion
    }
}